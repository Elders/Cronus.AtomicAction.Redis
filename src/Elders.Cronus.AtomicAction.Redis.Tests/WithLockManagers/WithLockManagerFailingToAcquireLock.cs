using System;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WithLockManagers
{
    public abstract class WithLockManagerFailingToAcquireLock
    {
        Establish context = () =>
        {
            lockManager = A.Fake<IAggregateRootLock>();
            A.CallTo(() => lockManager.Lock(A<IAggregateRootId>._, A<TimeSpan>._)).Returns(null);
            service = TestAtomicActionFactory.New(lockManager, A.Fake<IRevisionStore>());
        };

        protected static IAggregateRootLock lockManager;
        protected static IAggregateRootAtomicAction service;
    }
}
