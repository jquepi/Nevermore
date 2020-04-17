using System;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Nevermore.Advanced;
using Nevermore.Advanced.Hooks;
using Nevermore.Advanced.InstanceTypeResolvers;
using Nevermore.Advanced.ReaderStrategies;
using Nevermore.Advanced.Serialization;
using Nevermore.Advanced.TypeHandlers;
using Nevermore.Mapping;
using Nevermore.RelatedDocuments;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nevermore
{
    public interface IRelationalStoreConfiguration
    {
        string ApplicationName { get; set; }
        string ConnectionString { get; }
        IDocumentMapRegistry Mappings { get; }
        IDocumentSerializer Serializer { get; set; }
        IRelatedDocumentStore RelatedDocumentStore { get; set; }
        int KeyBlockSize { get; set; }
        IReaderStrategyRegistry ReaderStrategyRegistry { get; }
        ITypeHandlerRegistry TypeHandlerRegistry { get; }
        IInstanceTypeRegistry InstanceTypeRegistry { get; }

        /// <summary>
        /// MARS: https://docs.microsoft.com/en-us/sql/relational-databases/native-client/features/using-multiple-active-result-sets-mars?view=sql-server-ver15
        /// </summary>
        bool ForceMultipleActiveResultSets { get; set; }

        ISqlCommandFactory CommandFactory { get; set; }
    }
    
    public class RelationalStoreConfiguration : IRelationalStoreConfiguration
    {
        readonly Lazy<string> connectionString;

        public RelationalStoreConfiguration(string connectionString) : this(() => connectionString)
        {
        }

        public RelationalStoreConfiguration(Func<string> connectionStringFunc)
        {
            CommandFactory = new SqlCommandFactory();
            Mappings = new DocumentMapRegistry();
            KeyBlockSize = NevermoreDefaults.DefaultKeyBlockSize;
            InstanceTypeRegistry = new InstanceTypeRegistry();
            RelatedDocumentStore = new EmptyRelatedDocumentStore();
            
            this.UseJsonNetSerialization(s => {});
            
            ReaderStrategyRegistry = new ReaderStrategyRegistry();
            ReaderStrategyRegistry.Register(new DocumentReaderStrategy(this));
            ReaderStrategyRegistry.Register(new ValueTupleReaderStrategy(this));
            ReaderStrategyRegistry.Register(new PlainClassReaderStrategy(this));
            ReaderStrategyRegistry.Register(new PrimitiveReaderStrategy(this));
            
            HookRegistry = new HookRegistry();

            TypeHandlerRegistry = new TypeHandlerRegistry();
            
            connectionString = new Lazy<string>(() =>
            {
                var result = connectionStringFunc();
                return InitializeConnectionString(result);
            });
        }
        
        public string ApplicationName { get; set; }

        public string ConnectionString => connectionString.Value;

        public IDocumentMapRegistry Mappings { get; }
        
        public IDocumentSerializer Serializer { get; set; }
        
        public IRelatedDocumentStore RelatedDocumentStore { get; set; }
        
        public int KeyBlockSize { get; set; }
        
        public IReaderStrategyRegistry ReaderStrategyRegistry { get; }
        
        public ITypeHandlerRegistry TypeHandlerRegistry { get; }
        public IInstanceTypeRegistry InstanceTypeRegistry { get; }

        /// <summary>
        /// MARS: https://docs.microsoft.com/en-us/sql/relational-databases/native-client/features/using-multiple-active-result-sets-mars?view=sql-server-ver15
        /// </summary>
        public bool ForceMultipleActiveResultSets { get; set; }
        
        public ISqlCommandFactory CommandFactory { get; set; }
        public IHookRegistry HookRegistry { get; private set; }

        string InitializeConnectionString(string sqlConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(sqlConnectionString);
            if (ApplicationName != null) builder.ApplicationName = ApplicationName;
            if (ForceMultipleActiveResultSets) builder.MultipleActiveResultSets = true;

            OverrideValueIfNotSet(builder, nameof(builder.ConnectTimeout), NevermoreDefaults.DefaultConnectTimeoutSeconds);
            OverrideValueIfNotSet(builder, nameof(builder.ConnectRetryCount), NevermoreDefaults.DefaultConnectRetryCount);
            OverrideValueIfNotSet(builder, nameof(builder.ConnectRetryInterval), NevermoreDefaults.DefaultConnectRetryInterval);

            return builder.ToString();
        }

        static void OverrideValueIfNotSet(SqlConnectionStringBuilder connectionStringBuilder, string propertyName, object overrideValue)
        {
            var defaultConnectionStringBuilder = new SqlConnectionStringBuilder();

            var property = connectionStringBuilder.GetType().GetRuntimeProperty(propertyName);

            var defaultValue = property.GetValue(defaultConnectionStringBuilder);
            var currentValue = property.GetValue(connectionStringBuilder);

            if (Equals(defaultValue, currentValue))
            {
                property.SetValue(connectionStringBuilder, overrideValue);
            }
        }
    }
}