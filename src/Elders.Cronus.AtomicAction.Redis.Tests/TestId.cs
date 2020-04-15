using System;

namespace Elders.Cronus.AtomicAction.Redis.Tests
{
    public class TestId : AggregateRootId
    {
        public TestId() : this(Guid.NewGuid()) { }
        public TestId(Guid id) : base(id.ToString(), "redis-test", "elders") { }
    }
}
