using System;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.AggregateRootLock
{
    public class RedisAggregateRootLock : ILock
    {
        private IRedisLockManager lockManager;

        public RedisAggregateRootLock(IRedisLockManager lockManager)
        {
            if (ReferenceEquals(null, lockManager)) throw new ArgumentNullException(nameof(lockManager));

            this.lockManager = lockManager;
        }

        public bool IsLocked(string resource)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

            return lockManager.IsLocked(resource);
        }

        public bool Lock(string resource, TimeSpan ttl)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

            var lockresult = lockManager.Lock(resource, ttl);

            return lockresult;
        }

        public void Unlock(string resource)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

            lockManager.Unlock(resource);
        }
    }
}
