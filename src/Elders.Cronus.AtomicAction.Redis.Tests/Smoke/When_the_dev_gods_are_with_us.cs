using System;
using System.Threading.Tasks;
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
        Establish context = async () =>
        {
            id = new TestId();
            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.LockAsync(A<string>._, A<TimeSpan>._)).Returns(true);

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.PrepareRevisionAsync(id.ToBase64(), 2)).Returns(new Result<int>(1));

            options = new RedisAtomicActionOptionsMonitorMock().CurrentValue;
            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = async () => result = await service.ExecuteAsync(id, revision, task);

        It should_return__true__as_a_result = () => result.IsSuccessful.ShouldBeTrue();
        It should_not_have_an_exception_recorded = () => result.Errors.ShouldBeEmpty();

        It should_try_to_stored_the_current_revision = () =>
            A.CallTo(() => revisionStore.PrepareRevisionAsync(id.ToBase64(), revision))
                .MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();

        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.UnlockAsync(id.ToBase64())).MustHaveHappened();

        static int revision = 2;
        static TestId id;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Func<Task> task = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted = false;
        static RedisAtomicActionOptions options;
    }
}
