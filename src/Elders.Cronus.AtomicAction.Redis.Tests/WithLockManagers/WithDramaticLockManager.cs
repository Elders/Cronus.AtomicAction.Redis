using System;
using Elders.Cronus.DomainModeling;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WithLockManagers
{
    public abstract class WithDramaticLockManager
    {
        Establish context = () =>
        {
            lockManager = A.Fake<IAggregateRootLock>();
            A.CallTo(() => lockManager.Lock(A<IAggregateRootId>._, A<TimeSpan>._)).Throws(new Exception(message));
            service = TestAtomicActionFactory.New(lockManager, A.Fake<IRevisionStore>());
        };

        protected static IAggregateRootLock lockManager;
        protected static IAggregateRootAtomicAction service;
        protected static string message = "drama";
    }
}