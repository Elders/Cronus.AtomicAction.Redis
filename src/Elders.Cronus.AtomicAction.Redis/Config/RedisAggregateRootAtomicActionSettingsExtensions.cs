using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.Config
{

    public static class RedisAggregateRootAtomicActionSettingsExtensions
    {
        public static T SetLockEndPoints<T>(this T self, IEnumerable<IPEndPoint> endPoints)
            where T : IRedisAggregateRootAtomicActionSettings
        {
            self.EndPoints = endPoints;
            return self;
        }

        public static T SetLockClockDriveFactor<T>(this T self, double clockDriveFactor)
            where T : IRedisAggregateRootAtomicActionSettings
        {
            self.ClockDriveFactor = clockDriveFactor;
            return self;
        }

        public static T SetLockRetryCount<T>(this T self, int lockRetryCount)
            where T : IRedisAggregateRootAtomicActionSettings
        {
            self.LockRetryCount = lockRetryCount;
            return self;
        }

        public static T SetLockRetryDelay<T>(this T self, TimeSpan lockRetryDelay)
            where T : IRedisAggregateRootAtomicActionSettings
        {
            self.LockRetryDelay = lockRetryDelay;
            return self;
        }

        public static T SetLockTtl<T>(this T self, TimeSpan ttl) where T : IRedisAggregateRootAtomicActionSettings
        {
            self.LockTtl = ttl;
            return self;
        }

        public static T UseRedis<T>(this T self, Action<RedisAggregateRootAtomicActionSettings> configure)
            where T : IAggregateRootAtomicActionSettings
        {
            var settings = new RedisAggregateRootAtomicActionSettings();

            if (ReferenceEquals(null, configure) == false)
                configure(settings);

            var parsed = settings as IRedisAggregateRootAtomicActionSettings;

            var redLockOptions = new RedLockOptions
            {
                ClockDriveFactor = parsed.ClockDriveFactor,
                LockRetryCount = parsed.LockRetryCount,
                LockRetryDelay = parsed.LockRetryDelay
            };

            if (ReferenceEquals(null, parsed.EndPoints) || parsed.EndPoints.Any() == false)
                throw new Exception("Redis end points not specified.");

            var redLock = new RedisLockManager(redLockOptions, parsed.EndPoints);
            var redisAggregateRootLock = new RedisAggregateRootLock(redLock);

            var revisionStore = new RedisRevisionStore(parsed.EndPoints);
            var redisAtomicActionOptions = new RedisAtomicActionOptions { LockTtl = parsed.LockTtl };

            self.AggregateRootAtomicAtion =
                new RedisAggregateRootAtomicAction(redisAggregateRootLock, revisionStore, redisAtomicActionOptions);

            return self;
        }
    }
}