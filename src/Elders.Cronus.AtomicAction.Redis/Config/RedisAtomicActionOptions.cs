using System;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAtomicActionOptions
    {
        public RedisAtomicActionOptions(IConfiguration configuration)
        {
            LockTtl = GetValue(configuration, "cronus_atomicaction_redis_ttl_lock_ms", TimeSpan.FromMilliseconds(1000));
            ShorTtl = GetValue(configuration, "cronus_atomicaction_redis_ttl_short_ms", TimeSpan.FromMilliseconds(1000));
            LongTtl = GetValue(configuration, "cronus_atomicaction_redis_ttl_long_ms", TimeSpan.FromMilliseconds(300000));
        }

        RedisAtomicActionOptions() { }

        private static RedisAtomicActionOptions defaults = new RedisAtomicActionOptions()
        {
            LockTtl = TimeSpan.FromSeconds(1),
            ShorTtl = TimeSpan.FromSeconds(1),
            LongTtl = TimeSpan.FromMinutes(5)
        };

        public TimeSpan LockTtl { get; set; }

        public TimeSpan ShorTtl { get; set; }

        public TimeSpan LongTtl { get; set; }

        public static RedisAtomicActionOptions Defaults { get { return defaults; } }

        TimeSpan GetValue(IConfiguration configuration, string key, TimeSpan defaultValue)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value)) return defaultValue;

            return TimeSpan.FromMilliseconds(double.Parse(value));
        }
    }
}
