using System;
using Elders.Cronus.AtomicAction.Redis.Tests.WithLockManagers;
using FakeItEasy;
using Machine.Specifications;
using Elders.Cronus.Userfull;
using System.Threading.Tasks;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenLockManagerFails
{
    [Subject("Redis Atomic Action")]
    public class When_lock_cannot_be_acquired : WithLockManagerFailingToAcquireLock
    {
        Because of = async () => result = await service.ExecuteAsync(A.Fake<IAggregateRootId>(), 1, action);

        It should_return__false__as_a_result = () => result.IsSuccessful.ShouldBeFalse();
        It should_not_have_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_not_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.UnlockAsync(A<string>.Ignored)).MustNotHaveHappened();

        static Result<bool> result;
        static Func<Task> action = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted = false;
    }
}
