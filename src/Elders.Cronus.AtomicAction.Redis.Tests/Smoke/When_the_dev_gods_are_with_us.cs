using System;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.Smoke
{
    [Subject("Redis Atomic Action")]
    public class When_the_dev_gods_are_with_us
    {
        Establish context = () =>
        {
            id = new TestId();
            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(Convert.ToBase64String(id.RawId), A<TimeSpan>.Ignored)).Returns(true);

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(new Result<bool>(false));
            A.CallTo(() => revisionStore.GetRevision(id)).Returns(new Result<int>(revision - 1));

            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = () => result = service.Execute(id, revision, action);

        It should_return__true__as_a_result = () => result.IsSuccessful.ShouldBeTrue();
        It should_not_have_an_exception_recorded = () => result.Errors.ShouldBeEmpty();

        It should_try_to_stored_the_previous_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, revision - 1, RedisAtomicActionOptions.Defaults.ShorTtl))
                .MustHaveHappened();

        It should_try_to_increment_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, revision, RedisAtomicActionOptions.Defaults.ShorTtl))
                .MustHaveHappened();

        It should_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, revision, RedisAtomicActionOptions.Defaults.LongTtl))
                .MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(Convert.ToBase64String(id.RawId))).MustHaveHappened();

        static int revision = 2;
        static TestId id;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
    }
}
