using System;
using System.Net;
using Elders.Cronus.AtomicAction.Redis;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.DomainModeling;
using RedLock;
using StackExchange.Redis;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1001);

            var endPoints = new[] { endPoint1 };

            var connectionString = "docker-local.com:6379,abortConnect=False";

            var redlock = new RedisLockManager(connectionString);
            var aggregateRootLock = new RedisAggregateRootLock(redlock);
            var revisionStore = new RedisRevisionStore(connectionString);

            var options = new RedisAtomicActionOptions();
            options.LockTtl = TimeSpan.FromSeconds(1);
            options.ShorTtl = TimeSpan.FromSeconds(1);

            var atomicAction = new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore, options);
            var id = new HeadquarterId("e0846069-2730-4d3c-bc80-470d6a521d99", "elders");

            var result = atomicAction.Execute(id, 1, () => { });

            Console.WriteLine(result.IsSuccessful);
            Console.WriteLine(result.Value);
        }
    }

    public class HeadquarterId : StringTenantId
    {
        HeadquarterId() { }

        public HeadquarterId(StringTenantId id) : base(id, "Headquarter") { }

        public HeadquarterId(string id, string tenant) : base(id, "Headquarter", tenant) { }
    }

    class TestId : GuidId
    {
        public TestId() : base(Guid.Parse("e0846069-2730-4d3c-bc80-470d6a521d99"), "testid")
        {
        }
    }
}
