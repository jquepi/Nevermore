using System;
using Nevermore.Util;

namespace Nevermore.Transient
{
    /// <summary>
    /// Represents a retry strategy with a specified number of retry attempts and a default, fixed time interval between retries.
    /// </summary>
    public class FixedInterval : RetryStrategy
    {
        readonly int _retryCount;

        readonly TimeSpan _retryInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nevermore.Transient.FixedInterval" /> class.
        /// </summary>
        public FixedInterval() : this(DefaultClientRetryCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nevermore.Transient.FixedInterval" /> class with the specified number of retry attempts.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        public FixedInterval(int retryCount) : this(retryCount, DefaultRetryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nevermore.Transient.FixedInterval" /> class with the specified number of retry attempts and time interval.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The time interval between retries.</param>
        public FixedInterval(int retryCount, TimeSpan retryInterval) : this(null, retryCount, retryInterval, DefaultFirstFastRetry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nevermore.Transient.FixedInterval" /> class with the specified number of retry attempts, time interval, and retry strategy.
        /// </summary>
        /// <param name="name">The retry strategy name.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The time interval between retries.</param>
        public FixedInterval(string name, int retryCount, TimeSpan retryInterval) : this(name, retryCount, retryInterval, DefaultFirstFastRetry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nevermore.Transient.FixedInterval" /> class with the specified number of retry attempts, time interval, retry strategy, and fast start option.
        /// </summary>
        /// <param name="name">The retry strategy name.</param>
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The time interval between retries.</param>
        /// <param name="firstFastRetry">true to immediately retry in the first attempt; otherwise, false. The subsequent retries will remain subject to the configured retry interval.</param>
        public FixedInterval(string name, int retryCount, TimeSpan retryInterval, bool firstFastRetry) : base(name, firstFastRetry)
        {
            Guard.ArgumentNotNegativeValue(retryCount, "retryCount");
            Guard.ArgumentNotNegativeValue(retryInterval.Ticks, "retryInterval");
            _retryCount = retryCount;
            _retryInterval = retryInterval;
        }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>The ShouldRetry delegate.</returns>
        public override ShouldRetry GetShouldRetry()
        {
            if (_retryCount == 0)
            {
                return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    interval = TimeSpan.Zero;
                    return false;
                };
            }
            return delegate(int currentRetryCount, Exception lastException, out TimeSpan interval)
            {
                if (currentRetryCount < this._retryCount)
                {
                    interval = this._retryInterval;
                    return true;
                }
                interval = TimeSpan.Zero;
                return false;
            };
        }
    }
}