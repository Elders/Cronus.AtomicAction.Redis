using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.AtomicAction.Redis.Tests
{
    public class TestId : GuidId
    {
        public TestId() : this(Guid.NewGuid()) { }
        public TestId(Guid id) : base(id, "redis-test") { }
    }
}