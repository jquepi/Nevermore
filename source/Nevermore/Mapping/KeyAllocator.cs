using System;
using System.Collections.Concurrent;
using System.Data;
using Nevermore.Transient;

namespace Nevermore.Mapping
{
    public class KeyAllocator : IKeyAllocator
    {
        readonly IRelationalStore store;
        readonly int blockSize;
        readonly ConcurrentDictionary<string, Allocation> allocations = new ConcurrentDictionary<string, Allocation>(StringComparer.OrdinalIgnoreCase);

        public KeyAllocator(IRelationalStore store, int blockSize)
        {
            this.store = store;
            this.blockSize = blockSize;
        }

        public void Reset()
        {
            allocations.Clear();
        }

        public int NextId(string tableName)
        {
            var allocation = allocations.GetOrAdd(tableName, _ => new Allocation(store, tableName, blockSize));
            return allocation.Next();
        }

        class Allocation
        {
            readonly IRelationalStore store;
            readonly string collectionName;
            readonly int blockSize;
            readonly object sync = new object();
            volatile int blockStart;
            volatile int blockNext;
            volatile int blockFinish;

            public Allocation(IRelationalStore store, string collectionName, int blockSize)
            {
                this.store = store;
                this.collectionName = collectionName;
                this.blockSize = blockSize;
            }

            public int Next()
            {
                lock (sync)
                {
                    if (blockNext == blockFinish)
                        GetRetryPolicy().ExecuteAction(ExtendAllocation);

                    var result = blockNext;
                    blockNext++;

                    return result;
                }
            }

            RetryPolicy GetRetryPolicy()
            {
                return new RetryPolicy(new SqlDatabaseTransientErrorDetectionStrategy(),
                    TransientFaultHandling.FastIncremental)
                    .LoggingRetries("Extending key allocation");
            }

            void ExtendAllocation()
            {
                var max = GetNextMaxValue();
                SetRange(max);
            }

            void SetRange(int max)
            {
                var first = (max - blockSize) + 1;
                blockStart = first;
                blockNext = first;
                blockFinish = max + 1;
            }

            int GetNextMaxValue()
            {
                using var transaction = store.BeginWriteTransaction(IsolationLevel.Serializable, name: $"{nameof(KeyAllocator)}.{nameof(Allocation)}.{nameof(GetNextMaxValue)}");
                var parameters = new CommandParameterValues
                {
                    {"collectionName", collectionName},
                    {"blockSize", blockSize}
                };
                parameters.CommandType = CommandType.StoredProcedure;

                var result = transaction.ExecuteScalar<int>("GetNextKeyBlock", parameters);
                transaction.Commit();
                return result;
            }

            public override string ToString()
            {
                return $"{blockStart} to {blockNext} (next: {blockFinish})";
            }
        }
    }
}