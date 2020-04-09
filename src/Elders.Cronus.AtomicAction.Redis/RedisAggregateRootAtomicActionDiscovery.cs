using System.Collections.Generic;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicActionDiscovery : DiscoveryBase<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(context), services => services.AddOptions<RedisAtomicActionOptions, RedisAtomicActionOptionsProvider>());
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(RedisAggregateRootAtomicAction), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(RedLock.IRedisLockManager), x =>
            {
                var options = x.GetRequiredService<IOptionsMonitor<RedisAtomicActionOptions>>();
                var redLockOptions = new RedLock.RedLockOptions
                {
                    ClockDriveFactor = options.CurrentValue.ClockDriveFactor,
                    LockRetryCount = options.CurrentValue.LockRetryCount,
                    LockRetryDelay = options.CurrentValue.LockRetryDelay
                };

                return new RedLock.RedisLockManager(redLockOptions, options.CurrentValue.ConnectionString);
            }, ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ILock), typeof(AggregateRootLock.RedisAggregateRootLock), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IRevisionStore), typeof(RedisRevisionStore), ServiceLifetime.Singleton);
        }
    }
}
