using System;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public interface IRedisAggregateRootAtomicActionSettings
    {
        TimeSpan LockTtl { get; set; }

        TimeSpan ShorTtl { get; set; }

        TimeSpan LongTtl { get; set; }

        string ConnectionString { get; set; }

        double ClockDriveFactor { get; set; }

        int LockRetryCount { get; set; }

        TimeSpan LockRetryDelay { get; set; }
    }
}
