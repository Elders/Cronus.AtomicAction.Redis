using System;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private IRevisionStore revisionStore;
        private ILock aggregateRootLock;
        private RedisAtomicActionOptions options;
        private readonly ILogger<RedisAggregateRootAtomicAction> logger;

        public RedisAggregateRootAtomicAction(ILock aggregateRootLock, IRevisionStore revisionStore, IOptionsMonitor<RedisAtomicActionOptions> options, ILogger<RedisAggregateRootAtomicAction> logger)
        {
            if (aggregateRootLock is null) throw new ArgumentNullException(nameof(aggregateRootLock));
            if (revisionStore is null) throw new ArgumentNullException(nameof(revisionStore));
            if (ReferenceEquals(null, options)) throw new ArgumentNullException(nameof(options));

            this.aggregateRootLock = aggregateRootLock;
            this.revisionStore = revisionStore;
            this.options = options.CurrentValue;
            this.logger = logger;
        }

        public async Task<Result<bool>> ExecuteAsync(IAggregateRootId arId, int aggregateRootRevision, Func<Task> action)
        {
            var lockResult = await LockAsync(arId, options.LockTtl).ConfigureAwait(false);
            if (lockResult.IsNotSuccessful)
                return Result.Error($"Lock failed becouse of: {lockResult.Errors?.MakeJustOneException()}");

            try
            {

                var canExecuteActionResult = CanExecuteAction(arId, aggregateRootRevision);
                if (canExecuteActionResult.IsSuccessful == true)
                {
                    var actionResult = await ExecuteActionAsync(action).ConfigureAwait(false);

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
                logger.ErrorException(ex, () => "Unable to execute action");
                return Result.Error(ex);
            }
            finally
            {
                await UnlockAsync(lockResult.Value).ConfigureAwait(false);
            }
        }

        private async Task<Result<string>> LockAsync(IAggregateRootId arId, TimeSpan ttl)
        {
            try
            {
                var resource = Convert.ToBase64String(arId.RawId);

                if (await aggregateRootLock.LockAsync(resource, ttl).ConfigureAwait(false) == false)
                    return new Result<string>().WithError($"Failed to lock aggregate with id: {arId.Value}");

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

        private async Task<Result<bool>> ExecuteActionAsync(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
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

        private async Task UnlockAsync(string resource)
        {
            if (string.IsNullOrEmpty(resource)) return;

            try
            {
                await aggregateRootLock.UnlockAsync(resource).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Unable to unlock");
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
