using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;

namespace Elders.Cronus.AtomicAction.Redis.Tests
{
    public static class TestAtomicActionFactory
    {
        public static IAggregateRootAtomicAction New(ILock aggregateRootLock, IRevisionStore revisionStore)
        {
            return new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore, RedisAtomicActionOptions.Defaults);
        }
    }
}
