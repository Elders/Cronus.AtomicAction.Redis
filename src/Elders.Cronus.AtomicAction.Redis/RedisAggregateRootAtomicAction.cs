using System;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.Logging;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(RedisAggregateRootAtomicAction));

        private IRevisionStore revisionStore;

        private ILock aggregateRootLock;

        private RedisAtomicActionOptions options;

        public RedisAggregateRootAtomicAction(ILock aggregateRootLock, IRevisionStore revisionStore, RedisAtomicActionOptions options)
        {
            if (ReferenceEquals(null, aggregateRootLock)) throw new ArgumentNullException(nameof(aggregateRootLock));
            if (ReferenceEquals(null, revisionStore)) throw new ArgumentNullException(nameof(revisionStore));
            if (ReferenceEquals(null, options)) throw new ArgumentNullException(nameof(options));

            this.aggregateRootLock = aggregateRootLock;
            this.revisionStore = revisionStore;
            this.options = options;
        }

        public Result<bool> Execute(IAggregateRootId arId, int aggregateRootRevision, Action action)
        {
            var lockResult = Lock(arId, options.LockTtl);
            if (lockResult.IsNotSuccessful)
                return Result.Error($"Lock failed becouse of: {lockResult.Errors?.MakeJustOneException()}");

            try
            {

                var canExecuteActionResult = CanExecuteAction(arId, aggregateRootRevision);
                if (canExecuteActionResult.IsSuccessful == true)
                {
                    var actionResult = ExecuteAction(action);

                    if (actionResult.IsNotSuccessful)
                    {
                        Rollback(arId, aggregateRootRevision - 1);
                        return Result.Error($"Action faile becouse of: {actionResult.Errors?.MakeJustOneException()}");
                    }

                    PersistRevision(arId, aggregateRootRevision);

                    return actionResult;
                }

                return new Result<bool>(false).WithError("Unable to execute action").WithError(canExecuteActionResult.Errors?.MakeJustOneException());
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to execute action", ex);
                return Result.Error(ex);
            }
            finally
            {
                Unlock(lockResult.Value);
            }
        }

        private Result<string> Lock(IAggregateRootId arId, TimeSpan ttl)
        {
            try
            {
                var resource = Convert.ToBase64String(arId.RawId);

                if (aggregateRootLock.Lock(resource, ttl) == false)
                    return new Result<string>().WithError($"Failed to lock aggregate with id: {arId.Urn.Value}");

                return new Result<string>(resource);
            }
            catch (Exception ex)
            {
                return new Result<string>().WithError(ex);
            }
        }

        private Result<bool> CheckForExistingRevision(IAggregateRootId arId)
        {
            return revisionStore.HasRevision(arId);
        }

        private Result<bool> SavePreviouseRevison(IAggregateRootId arId, int revision)
        {
            return revisionStore.SaveRevision(arId, revision - 1, options.ShorTtl);
        }

        private Result<bool> PersistRevision(IAggregateRootId arId, int revision)
        {
            return revisionStore.SaveRevision(arId, revision, options.LongTtl);
        }

        private bool IsConsecutiveRevision(IAggregateRootId arId, int revision)
        {
            var storedRevisionResult = revisionStore.GetRevision(arId);
            return storedRevisionResult.IsSuccessful && storedRevisionResult.Value == revision - 1;
        }

        private Result<bool> IncrementRevision(IAggregateRootId arId, int newRevision)
        {
            return revisionStore.SaveRevision(arId, newRevision, options.ShorTtl);
        }

        private Result<bool> ExecuteAction(Action action)
        {
            try
            {
                action();
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Error(ex);
            }
        }

        private void Rollback(IAggregateRootId arId, int revision)
        {
            revisionStore.SaveRevision(arId, revision, options.LongTtl);
        }

        private void Unlock(string resource)
        {
            if (string.IsNullOrEmpty(resource)) return;

            try
            {
                aggregateRootLock.Unlock(resource);
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to unlock", ex);
            }
        }

        Result<bool> CanExecuteAction(IAggregateRootId arId, int aggregateRootRevision)
        {
            try
            {
                var existingRevisionResult = CheckForExistingRevision(arId);
                if (existingRevisionResult.IsNotSuccessful)
                {
                    return new Result<bool>(false).WithError("ExistingRevisionResult is false.").WithError(existingRevisionResult.Errors?.MakeJustOneException()); // false
                }

                if (existingRevisionResult.Value == false)
                {
                    var prevRevResult = SavePreviouseRevison(arId, aggregateRootRevision);

                    if (prevRevResult.IsNotSuccessful)
                        return new Result<bool>(false).WithError("PrevRevResult is false.").WithError(prevRevResult.Errors?.MakeJustOneException()); // false
                }

                var isConsecutiveRevision = IsConsecutiveRevision(arId, aggregateRootRevision);
                if (isConsecutiveRevision)
                {
                    return IncrementRevision(arId, aggregateRootRevision);  // true / false
                }

                return new Result<bool>(false).WithError("Revisions were not consecutive"); // false
            }
            catch (Exception ex)
            {
                return new Result<bool>(false).WithError(ex);
            }
        }

        public void Dispose()
        {
        }
    }
}
