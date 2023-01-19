﻿using System;
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
            if (options is null) throw new ArgumentNullException(nameof(options));

            this.aggregateRootLock = aggregateRootLock;
            this.revisionStore = revisionStore;
            this.options = options.CurrentValue;
            this.logger = logger;
        }

        public async Task<Result<bool>> ExecuteAsync(AggregateRootId arId, int aggregateRootRevision, Func<Task> action)
        {
            string resource = Convert.ToBase64String(arId.RawId);

            bool isArLocked = await aggregateRootLock.LockAsync(resource, options.LockTtl).ConfigureAwait(false);
            if (isArLocked)
            {
                Result<int> lockedPrevRevision = await revisionStore.PrepareRevisionAsync(resource, aggregateRootRevision).ConfigureAwait(false);
                if (lockedPrevRevision.IsSuccessful)
                {
                    if (lockedPrevRevision.Value == 0 || lockedPrevRevision.Value + 1 == aggregateRootRevision)
                    {
                        Result<bool> actionResult = await ExecuteActionAsync(action).ConfigureAwait(false);
                        if (actionResult.IsSuccessful)
                        {
                            await aggregateRootLock.UnlockAsync(resource).ConfigureAwait(false);
                            return actionResult;
                        }
                    }

                    await revisionStore.SaveRevisionAsync(resource, lockedPrevRevision.Value, options.LongTtl).ConfigureAwait(false);
                }
                else
                {
                    await aggregateRootLock.UnlockAsync(resource).ConfigureAwait(false);
                    return new Result<bool>(false).WithError(lockedPrevRevision.Errors?.MakeJustOneException());
                }

                await aggregateRootLock.UnlockAsync(resource).ConfigureAwait(false);
            }

            return new Result<bool>(false).WithError("Unable to execute action");
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

        public void Dispose()
        {
        }
    }
}
