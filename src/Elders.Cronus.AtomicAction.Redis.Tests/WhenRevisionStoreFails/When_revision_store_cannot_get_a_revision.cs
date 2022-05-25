using System;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;

namespace Elders.Cronus.AtomicAction.Redis.Tests.WhenRevisionStoreFails
{
    [Subject("Redis Atomic Action")]
    public class When_revision_store_cannot_get_a_revision
    {
        Establish context = () =>
        {
            id = new TestId();
            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.LockAsync(Convert.ToBase64String(id.RawId), A<TimeSpan>.Ignored)).Returns(true);

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Throws(new Exception(message));

            options = new RedisAtomicActionOptionsMonitorMock().CurrentValue;
            service = TestAtomicActionFactory.New(lockManager, revisionStore);
        };

        Because of = async () => result = await service.ExecuteAsync(id, 1, action);

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.UnlockAsync(Convert.ToBase64String(id.RawId))).MustHaveHappened();
        It should_not_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, options.LongTtl))
                .MustNotHaveHappened();

        static string message = "cannot get revision";
        static TestId id;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Func<Task> action = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted = false;
        static RedisAtomicActionOptions options;
    }
}
