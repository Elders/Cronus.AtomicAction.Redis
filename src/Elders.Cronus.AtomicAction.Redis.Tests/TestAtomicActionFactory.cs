using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;

namespace Elders.Cronus.AtomicAction.Redis.Tests
{
    public static class TestAtomicActionFactory
    {
        public static IAggregateRootAtomicAction New(IAggregateRootLock aggregateRootLock, IRevisionStore revisionStore)
        {
            return new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore);
        }
    }
}