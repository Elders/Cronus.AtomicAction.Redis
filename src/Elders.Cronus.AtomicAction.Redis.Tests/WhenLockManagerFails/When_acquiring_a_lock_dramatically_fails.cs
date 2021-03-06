﻿using System;
using Elders.Cronus.AtomicAction.Redis.Tests.WithLockManagers;
using FakeItEasy;
using Machine.Specifications;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenLockManagerFails
{
    [Subject("Redis Atomic Action")]
    public class When_acquiring_a_lock_dramatically_fails : WithDramaticLockManager
    {
        Because of = () => result = service.Execute(A.Fake<IAggregateRootId>(), 1, action);

        It should_return__false__as_a_result = () => result.IsSuccessful.ShouldBeFalse();
        It should_have_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_not_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(A<string>.Ignored)).MustNotHaveHappened();

        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
    }
}
