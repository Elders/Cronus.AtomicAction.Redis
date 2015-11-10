using System;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using RedLock;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenTheActionFails
{
    [Subject("Redis Atomic Action")]
    public class When_the_action_throws_an_exception
    {
        Establish context = () =>
        {
            mutex = A.Fake<Mutex>();
            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Returns(new Result<int>(1));

            lockManager = A.Fake<IAggregateRootLock>();
            A.CallTo(() => lockManager.Lock(id, A<TimeSpan>.Ignored)).Returns(mutex);

            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = () => result = service.Execute(id, 2, action);

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_produced = () => result.Errors.ShouldNotBeEmpty();
        It should_try_to_increment_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 2, RedisAtomicActionOptions.Defaults.ShorTtl))
                .MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();
        It should_try_to_decrement_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, RedisAtomicActionOptions.Defaults.LongTtl))
                .MustHaveHappened();

        It should_not_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 2, RedisAtomicActionOptions.Defaults.LongTtl))
                .MustNotHaveHappened();


        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(mutex)).MustHaveHappened();

        static Mutex mutex;
        static TestId id = new TestId();
        static IAggregateRootLock lockManager;
        static IRevisionStore revisionStore;
        static Result<bool> result;
        static IAggregateRootAtomicAction service;
        static bool actionExecuted = false;
        static Action action = () =>
        {
            actionExecuted = true;
            throw new Exception();
        };
    }
}
