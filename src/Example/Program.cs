using System;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.Redis;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.RedLock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<RedisAtomicActionOptions>(configuration);
            services.Configure<RedLockOptions>(configuration);
            services.AddOptions<RedisAtomicActionOptions, RedisAtomicActionOptionsProvider>();
            services.AddOptions<RedLockOptions, RedLockOptionsProvider>();
            services.AddTransient<IAggregateRootAtomicAction, RedisAggregateRootAtomicAction>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();
            services.AddTransient<ILock, RedisAggregateRootLock>();
            services.AddSingleton<IRevisionStore, RedisRevisionStore>();
            var serviceProvider = services.BuildServiceProvider();

            var atomicAction = serviceProvider.GetRequiredService<IAggregateRootAtomicAction>();
            var id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            var revision = 1;

            while (true)
            {
                System.Threading.Thread.Sleep(100);
                Console.Clear();
                var result = atomicAction.Execute(id, revision++, () =>
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay}-{id}");
                });

                Console.WriteLine($"{DateTime.Now.TimeOfDay}-{result.IsSuccessful}");
            }
        }
    }

    public class HeadquarterId : StringTenantId
    {
        HeadquarterId() { }

        public HeadquarterId(StringTenantId id) : base(id, "Headquarter") { }

        public HeadquarterId(string id, string tenant) : base(id, "Headquarter", tenant) { }
    }

    class TestId : StringTenantId
    {
        public TestId() : base("e0846069-2730-4d3c-bc80-470d6a521d99", "testid", "elders")
        {
        }
    }
}
