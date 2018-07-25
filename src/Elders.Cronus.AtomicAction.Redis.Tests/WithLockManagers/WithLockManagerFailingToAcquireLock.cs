using System;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WithLockManagers
{
    public abstract class WithLockManagerFailingToAcquireLock
    {
        Establish context = () =>
        {
            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(A<string>._, A<TimeSpan>._)).Returns(false);
            service = TestAtomicActionFactory.New(lockManager, A.Fake<IRevisionStore>());
        };

        protected static ILock lockManager;
        protected static IAggregateRootAtomicAction service;
    }
}
