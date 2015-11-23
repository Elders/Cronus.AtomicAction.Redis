using System;
using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction.Redis
{
    public class RedisAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private IRevisionStore revisionStore;

        private IAggregateRootLock aggregateRootLock;

        private RedisAtomicActionOptions options;

        public RedisAggregateRootAtomicAction(IAggregateRootLock aggregateRootLock, IRevisionStore revisionStore) :
            this(aggregateRootLock, revisionStore, RedisAtomicActionOptions.Defaults)
        {
        }

        public RedisAggregateRootAtomicAction(IAggregateRootLock aggregateRootLock,
                                              IRevisionStore revisionStore,
                                              RedisAtomicActionOptions options)
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
                return Result.Error("lock failed");

            try
            {
                if (CanExecuteAction(arId, aggregateRootRevision))
                {
                    var actionResult = ExecuteAction(action);

                    if (actionResult.IsNotSuccessful)
                    {
                        Rollback(arId, aggregateRootRevision - 1);

                        return Result.Error("action failed");
                    }

                    PersistRevision(arId, aggregateRootRevision);

                    return actionResult;
                }

                return Result.Error("unable to execute action");
            }
            catch (Exception ex)
            {
                // TODO log
                return Result.Error(ex);
            }
            finally
            {
                Unlock(lockResult.Value);
            }
        }

        private Result<object> Lock(IAggregateRootId arId, TimeSpan ttl)
        {
            object mutex;

            try
            {
                mutex = aggregateRootLock.Lock(arId, ttl);

                if (ReferenceEquals(null, mutex))
                    return new Result<object>().WithError("failed lock");

                return new Result<object>(mutex);
            }
            catch (Exception ex)
            {
                return new Result<object>().WithError(ex);
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
            revisionStore.SaveRevision(arId, revision, options.LongTtl); // TODO: save with long ttl
        }

        private void Unlock(object mutex)
        {
            if (ReferenceEquals(null, mutex)) return;

            try
            {
                aggregateRootLock.Unlock(mutex);
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }
        private bool CanExecuteAction(IAggregateRootId arId, int aggregateRootRevision)
        {
            try
            {
                var existingRevisionResult = CheckForExistingRevision(arId);

                if (existingRevisionResult.IsNotSuccessful)
                {
                    return false; // TODO: log
                }

                if (existingRevisionResult.Value == false)
                {
                    var prevRevResult = SavePreviouseRevison(arId, aggregateRootRevision);

                    if (prevRevResult.IsNotSuccessful)
                        return false;
                }

                var idConsecutiveRevision = IsConsecutiveRevision(arId, aggregateRootRevision);

                if (idConsecutiveRevision)
                {
                    return IncrementRevision(arId, aggregateRootRevision).IsSuccessful;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
        }
    }
}
