using System;
using Elders.Cronus;
using Elders.Cronus.AtomicAction.Redis;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using RedLock;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "docker-local.com:6379,abortConnect=False";

            var redlock = new RedisLockManager(connectionString);
            var aggregateRootLock = new RedisAggregateRootLock(redlock);
            var revisionStore = new RedisRevisionStore(connectionString);

            var options = new RedisAtomicActionOptions();
            options.LockTtl = TimeSpan.FromSeconds(1);
            options.ShorTtl = TimeSpan.FromSeconds(1);

            var atomicAction = new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore, options);
            var id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            var revision = 1;

            while (true)
            {
                System.Threading.Thread.Sleep(500);
                var result = atomicAction.Execute(id, revision++, () =>
                {
                    Console.WriteLine(id);
                });
                Console.WriteLine(result.IsSuccessful);
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
