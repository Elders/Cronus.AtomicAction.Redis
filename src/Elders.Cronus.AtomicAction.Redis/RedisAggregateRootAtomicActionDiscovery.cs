﻿using System.Collections.Generic;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Discoveries;
using Elders.RedLock;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicActionDiscovery : DiscoveryBase<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(context), services => services.AddOptions<RedisAtomicActionOptions, RedisAtomicActionOptionsProvider>()
                                                                                                           .AddOptions<RedLockOptions, RedLockOptionsProvider>());
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(RedisAggregateRootAtomicAction), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IRedisLockManager), typeof(RedisLockManager), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ILock), typeof(AggregateRootLock.RedisAggregateRootLock), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IRevisionStore), typeof(RedisRevisionStore), ServiceLifetime.Singleton);
        }
    }
}
