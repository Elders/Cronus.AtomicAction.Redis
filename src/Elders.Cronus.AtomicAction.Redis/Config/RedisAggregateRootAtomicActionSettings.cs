using System;
using System.Collections.Generic;
using System.Net;
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
        }

        TimeSpan IRedisAggregateRootAtomicActionSettings.LockTtl { get; set; }

        IEnumerable<IPEndPoint> IRedisAggregateRootAtomicActionSettings.EndPoints { get; set; }

        double IRedisAggregateRootAtomicActionSettings.ClockDriveFactor { get; set; }

        int IRedisAggregateRootAtomicActionSettings.LockRetryCount { get; set; }

        TimeSpan IRedisAggregateRootAtomicActionSettings.LockRetryDelay { get; set; }
    }
}
