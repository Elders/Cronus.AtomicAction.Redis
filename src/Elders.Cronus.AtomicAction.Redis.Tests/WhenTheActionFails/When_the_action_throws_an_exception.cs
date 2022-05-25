using System;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenTheActionFails
{
    [Subject("Redis Atomic Action")]
    public class When_the_action_throws_an_exception
    {
        Establish context = () =>
        {
            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Returns(new Result<int>(1));

            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.LockAsync(Convert.ToBase64String(id.RawId), A<TimeSpan>.Ignored)).Returns(true);

            options = new RedisAtomicActionOptionsMonitorMock().CurrentValue;
            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = async () => result = await service.ExecuteAsync(id, 2, action);

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_produced = () => result.Errors.ShouldNotBeEmpty();
        It should_try_to_increment_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 2, options.ShorTtl))
                .MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();
        It should_try_to_decrement_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, options.LongTtl))
                .MustHaveHappened();

        It should_not_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 2, options.LongTtl))
                .MustNotHaveHappened();


        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.UnlockAsync(Convert.ToBase64String(id.RawId))).MustHaveHappened();

        static TestId id = new TestId();
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static Result<bool> result;
        static IAggregateRootAtomicAction service;
        static bool actionExecuted = false;
        static RedisAtomicActionOptions options;
        static Func<Task> action = () =>
        {
            actionExecuted = true;
            throw new Exception();
        };
    }
}
