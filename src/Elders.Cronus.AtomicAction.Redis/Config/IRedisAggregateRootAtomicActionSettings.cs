using System;
using System.Collections.Generic;
using System.Net;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public interface IRedisAggregateRootAtomicActionSettings
    {
        TimeSpan LockTtl { get; set; }

        IEnumerable<IPEndPoint> EndPoints { get; set; }

        double ClockDriveFactor { get; set; }

        int LockRetryCount { get; set; }

        TimeSpan LockRetryDelay { get; set; }
    }
}