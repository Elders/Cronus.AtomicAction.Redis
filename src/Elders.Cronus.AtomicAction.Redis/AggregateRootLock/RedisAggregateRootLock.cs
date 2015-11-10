using System;
using Elders.Cronus.DomainModeling;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.AggregateRootLock
{
    public class RedisAggregateRootLock : IAggregateRootLock
    {
        private IRedisLockManager lockManager;

        public RedisAggregateRootLock(IRedisLockManager lockManager)
        {
            if (ReferenceEquals(null, lockManager)) throw new ArgumentNullException(nameof(lockManager));

            this.lockManager = lockManager;
        }

        public bool IsLocked(IAggregateRootId aggregateRootId)
        {
            return lockManager.IsLocked(aggregateRootId);
        }

        public object Lock(IAggregateRootId aggregateRootId, TimeSpan ttl)
        {
            if (ReferenceEquals(null, aggregateRootId)) throw new ArgumentNullException(nameof(aggregateRootId));

            var lockresult = lockManager.Lock(aggregateRootId, ttl);

            return lockresult.Locked ? lockresult.Mutex : null;
        }

        public void Unlock(object mutex)
        {
            if (ReferenceEquals(null, mutex)) return;

            lockManager.Unlock(mutex as Mutex);
        }

        public void Dispose()
        {
        }
    }
}
