using System.Collections.Generic;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicActionDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(RedisAggregateRootAtomicAction), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(RedLock.IRedisLockManager), new RedLock.RedisLockManager(context.Configuration["cronus_atomicaction_redis_connectionstring"]));
            yield return new DiscoveredModel(typeof(ILock), typeof(AggregateRootLock.RedisAggregateRootLock), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IRevisionStore), typeof(RedisRevisionStore), ServiceLifetime.Transient);
        }
    }
}
