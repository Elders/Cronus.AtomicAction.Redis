using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Discoveries;


namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicActionDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var discoveryResult = new DiscoveryResult<IAggregateRootAtomicAction>();
            discoveryResult.Models.Add(new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(RedisAggregateRootAtomicAction)));
            discoveryResult.Models.Add(new DiscoveredModel(typeof(RedLock.IRedisLockManager), typeof(RedLock.RedisLockManager), new RedLock.RedisLockManager(context.Configuration["cronus_atomicaction_redis_connectionstring"])));
            discoveryResult.Models.Add(new DiscoveredModel(typeof(ILock), typeof(AggregateRootLock.RedisAggregateRootLock)));
            discoveryResult.Models.Add(new DiscoveredModel(typeof(IRevisionStore), typeof(RedisRevisionStore)));

            return discoveryResult;
        }
    }
}
