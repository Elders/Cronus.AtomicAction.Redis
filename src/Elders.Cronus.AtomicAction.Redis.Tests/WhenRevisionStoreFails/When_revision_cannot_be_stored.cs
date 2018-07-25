using System;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenRevisionStoreFails
{
    [Subject("Redis Atomic Action")]
    public class When_revision_store_cannot_store_a_revision
    {
        Establish context = () =>
        {
            id = new TestId();
            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(Convert.ToBase64String(id.RawId), A<TimeSpan>.Ignored)).Returns(true);

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Returns(new Result<int>(0));
            A.CallTo(() => revisionStore.SaveRevision(id, A<int>.Ignored, A<TimeSpan?>.Ignored)).Throws(new Exception(message));

            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = () => result = service.Execute(id, 1, () => { });

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(Convert.ToBase64String(id.RawId))).MustHaveHappened();
        It should_not_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, RedisAtomicActionOptions.Defaults.LongTtl))
                .MustNotHaveHappened();

        static string message = "cannot save revision";
        static TestId id;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
    }
}
