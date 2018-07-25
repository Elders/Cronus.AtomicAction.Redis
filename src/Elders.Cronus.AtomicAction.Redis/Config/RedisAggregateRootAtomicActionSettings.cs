using System;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAggregateRootAtomicActionSettings : IRedisAggregateRootAtomicActionSettings
    {
        public RedisAggregateRootAtomicActionSettings()
        {
            this.SetLockClockDriveFactor(RedLockOptions.Default.ClockDriveFactor);
            this.SetLockRetryCount(RedLockOptions.Default.LockRetryCount);
            this.SetLockRetryDelay(RedLockOptions.Default.LockRetryDelay);
            this.SetLockTtl(RedisAtomicActionOptions.Defaults.LockTtl);
            this.SetShortTtl(RedisAtomicActionOptions.Defaults.ShorTtl);
            this.SetLongTtl(RedisAtomicActionOptions.Defaults.LongTtl);
        }

        TimeSpan IRedisAggregateRootAtomicActionSettings.LockTtl { get; set; }

        TimeSpan IRedisAggregateRootAtomicActionSettings.ShorTtl { get; set; }

        TimeSpan IRedisAggregateRootAtomicActionSettings.LongTtl { get; set; }

        string IRedisAggregateRootAtomicActionSettings.ConnectionString { get; set; }

        double IRedisAggregateRootAtomicActionSettings.ClockDriveFactor { get; set; }

        int IRedisAggregateRootAtomicActionSettings.LockRetryCount { get; set; }

        TimeSpan IRedisAggregateRootAtomicActionSettings.LockRetryDelay { get; set; }
    }
}
