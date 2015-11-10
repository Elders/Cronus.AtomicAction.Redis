using System;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAtomicActionOptions
    {
        private static RedisAtomicActionOptions defaults = new RedisAtomicActionOptions
        {
            LockTtl = TimeSpan.FromSeconds(1)
        };

        public TimeSpan LockTtl { get; set; }

        public static RedisAtomicActionOptions Defaults { get { return defaults; } }
    }
}