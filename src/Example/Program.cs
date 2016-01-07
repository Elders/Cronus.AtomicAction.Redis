using System;
using System.Net;
using Elders.Cronus.AtomicAction.Redis;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.DomainModeling;
using RedLock;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint1 = new IPEndPoint(IPAddress.Parse("192.168.99.100"), 1001);
            var endPoint2 = new IPEndPoint(IPAddress.Parse("192.168.99.100"), 1002);
            var endPoint3 = new IPEndPoint(IPAddress.Parse("192.168.99.100"), 1003);

            var endPoints = new[] { endPoint1, endPoint2, endPoint3 };

            var redlock = new RedisLockManager(endPoints);
            var aggregateRootLock = new RedisAggregateRootLock(redlock);
            var revisionStore = new RedisRevisionStore(endPoints);

            var options = new RedisAtomicActionOptions();
            options.LockTtl = TimeSpan.FromSeconds(1);
            options.ShorTtl = TimeSpan.FromSeconds(1);

            var atomicAction = new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore, options);
            var id = new TestId();

            var result = atomicAction.Execute(id, 1, () => { });

            Console.WriteLine(result.IsSuccessful);
            Console.WriteLine(result.Value);
        }
    }

    class TestId : GuidId
    {
        public TestId() : base(Guid.Parse("e0846069-2730-4d3c-bc80-470d6a521d99"), "testid")
        {
        }
    }
}
