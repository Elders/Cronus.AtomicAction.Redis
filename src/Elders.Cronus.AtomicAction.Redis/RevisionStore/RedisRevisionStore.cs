using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;
using StackExchange.Redis;

namespace Elders.Cronus.AtomicAction.Redis.RevisionStore
{
    public class RedisRevisionStore : IRevisionStore
    {
        private IList<ConnectionMultiplexer> connections;

        private int Quorum { get { return (connections.Count / 2) + 1; } }

        public RedisRevisionStore(IEnumerable<IPEndPoint> redisEndpoints)
        {
            connections = new List<ConnectionMultiplexer>();

            foreach (var endpoint in redisEndpoints)
            {
                // TODO: this will throw if an endpoint is unreachable
                connections.Add(ConnectionMultiplexer.Connect(endpoint.ToString()));
            }
        }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision)
        {
            return SaveRevision(aggregateRootId, revision, null);
        }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision, TimeSpan? expiry)
        {
            if (ReferenceEquals(null, aggregateRootId)) throw new ArgumentNullException(nameof(aggregateRootId));

            var revisionKey = CreateRedisRevisionKey(aggregateRootId);
            var nodesWeNeedToProceed = Quorum;

            foreach (var connection in connections)
            {
                try
                {
                    if (connection.GetDatabase().StringSet(revisionKey, string.Join(",", revision, DateTime.UtcNow), expiry))
                    {
                        nodesWeNeedToProceed--;
                    }
                }
                catch (Exception)
                {
                    // Current node is down. Continue trying to get a quorum.
                    continue;
                }
            }

            // This is a rollback; if we persisted the revision in some nodes but we do not have a quorum
            if (nodesWeNeedToProceed > 0 && connections.Any())
            {
                foreach (var connection in connections)
                {
                    connection.GetDatabase().KeyDelete(revisionKey);
                }

                return Result.Error($"Unable to store revision for '{aggregateRootId}'. Quorum: {Quorum}");
            }

            return Result.Success;
        }

        public Result<int> GetRevision(IAggregateRootId aggregateRootId)
        {
            var revisionKey = CreateRedisRevisionKey(aggregateRootId);
            var storedRevisions = new List<int>();

            foreach (var connection in connections)
            {
                try
                {
                    var value = connection.GetDatabase().StringGet(revisionKey);
                    var revisionValue = ((string)value).Split(',').First();
                    storedRevisions.Add(int.Parse(revisionValue));
                }
                catch (Exception)
                {
                    // Current node is down. Continue trying to get a quorum.
                    continue;
                }
            }

            var maxRevision = storedRevisions.Max();
            if (storedRevisions.Count(x => x == maxRevision) < Quorum)
                return new Result<int>(-1).WithError($"Inconsistent revisions for aggregate root '{aggregateRootId}'.");

            return new Result<int>(maxRevision);
        }

        public Result<bool> HasRevision(IAggregateRootId aggregateRootId)
        {
            var revisionKey = CreateRedisRevisionKey(aggregateRootId);
            var results = new List<bool>();

            foreach (var connection in connections)
            {
                try
                {
                    results.Add(connection.GetDatabase().KeyExists(revisionKey));
                }
                catch (Exception)
                {
                    // Current node is down. Continue trying to get a quorum.
                    results.Add(false);
                }
            }

            var hasRevision = results.Count(x => x == true) >= Quorum;

            return new Result<bool>(hasRevision);
        }

        public void Dispose()
        {
            if (connections != null)
            {
                foreach (var connection in connections)
                {
                    connection.Dispose();
                }

                connections = null;
            }
        }

        private string CreateRedisRevisionKey(IAggregateRootId aggregateRootId)
        {
            var stringRawId = Convert.ToBase64String(aggregateRootId.RawId);

            return string.Concat("revision-", stringRawId);
        }
    }
}
