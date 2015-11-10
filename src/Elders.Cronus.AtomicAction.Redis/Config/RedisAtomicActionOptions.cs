using System;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAtomicActionOptions
    {
        private static RedisAtomicActionOptions defaults = new RedisAtomicActionOptions
        {
            LockTtl = TimeSpan.FromSeconds(1),
            ShorTtl = TimeSpan.FromSeconds(1),
            LongTtl = TimeSpan.FromMinutes(5)
        };

        public TimeSpan LockTtl { get; set; }

        public TimeSpan ShorTtl { get; set; }

        public TimeSpan LongTtl { get; set; }

        public static RedisAtomicActionOptions Defaults { get { return defaults; } }
    }
}