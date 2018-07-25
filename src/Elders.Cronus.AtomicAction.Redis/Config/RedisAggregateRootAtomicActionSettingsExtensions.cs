using System;
using Elders.Cronus.AtomicAction.Config;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public static class RedisAggregateRootAtomicActionSettingsExtensions
    {
        public static T SetConnectionString<T>(this T self, string connectionString)
            where T : IRedisAggregateRootAtomicActionSettings
        {
            var options = connectionString;
            self.ConnectionString = options;
            return self;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Clock_drift
        /// <para>clock_drift = (expiry_milliseconds * clock_drive_factor) + 2</para>
        /// <para>http://redis.io/topics/distlock#safety-arguments</para>
        /// </summary>
        /// <typeparam name="T">IRedisAggregateRootAtomicActionSettings</typeparam>
        /// <param name="self">The extended instance</param>
        /// <param name="clockDriveFactor">The multiplication factor for the expiry milliseconds.</param>
        /// <returns>The extended instance</returns>
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

        public static T SetShortTtl<T>(this T self, TimeSpan ttl) where T : IRedisAggregateRootAtomicActionSettings
        {
            self.ShorTtl = ttl;
            return self;
        }

        public static T SetLongTtl<T>(this T self, TimeSpan ttl) where T : IRedisAggregateRootAtomicActionSettings
        {
            self.LongTtl = ttl;
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

            if (string.IsNullOrEmpty(parsed.ConnectionString))
                throw new Exception("Redis ConnectionString is not specified.");

            var redLock = new RedisLockManager(redLockOptions, parsed.ConnectionString);
            var redisAggregateRootLock = new RedisAggregateRootLock(redLock);

            var revisionStore = new RedisRevisionStore(parsed.ConnectionString);
            var redisAtomicActionOptions = new RedisAtomicActionOptions { LockTtl = parsed.LockTtl, ShorTtl = parsed.ShorTtl, LongTtl = parsed.LongTtl };

            self.AggregateRootAtomicAtion =
                new RedisAggregateRootAtomicAction(redisAggregateRootLock, revisionStore, redisAtomicActionOptions);

            return self;
        }
    }
}
