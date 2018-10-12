using System;
using System.Linq;
using Elders.Cronus.Userfull;
using StackExchange.Redis;

namespace Elders.Cronus.AtomicAction.Redis.RevisionStore
{
    public class RedisRevisionStore : IRevisionStore
    {
        private ConnectionMultiplexer connection;

        public RedisRevisionStore(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            var configurationOptions = ConfigurationOptions.Parse(connectionString);

            connection = ConnectionMultiplexer.Connect(configurationOptions);
        }

        public RedisRevisionStore(Microsoft.Extensions.Configuration.IConfiguration configuration) : this(configuration["cronus_atomicaction_redis_connectionstring"]) { }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision)
        {
            return SaveRevision(aggregateRootId, revision, null);
        }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision, TimeSpan? expiry)
        {
            if (ReferenceEquals(null, aggregateRootId)) throw new ArgumentNullException(nameof(aggregateRootId));

            if (connection.IsConnected == false)
                return Result.Error($"Unreachable endpoint '{connection.ClientName}'.");

            var revisionKey = CreateRedisRevisionKey(aggregateRootId);

            try
            {
                var result = connection.GetDatabase().StringSet(revisionKey, string.Join(",", revision, DateTime.UtcNow), expiry);

                return new Result<bool>(result);
            }
            catch (Exception ex)
            {
                return Result.Error(ex);
            }
        }

        public Result<int> GetRevision(IAggregateRootId aggregateRootId)
        {
            if (ReferenceEquals(null, aggregateRootId)) throw new ArgumentNullException(nameof(aggregateRootId));

            if (connection.IsConnected == false)
                return new Result<int>().WithError($"Unreachable endpoint '{connection.ClientName}'.");

            var revisionKey = CreateRedisRevisionKey(aggregateRootId);

            try
            {
                var value = connection.GetDatabase().StringGet(revisionKey);
                if (value.HasValue == false)
                    return new Result<int>().WithError($"Missing value for {revisionKey} '{connection.ClientName}'.");
                var revisionValue = ((string)value).Split(',').First();

                return new Result<int>(int.Parse(revisionValue));
            }
            catch (Exception ex)
            {
                return new Result<int>().WithError(ex);
            }
        }

        public Result<bool> HasRevision(IAggregateRootId aggregateRootId)
        {
            if (ReferenceEquals(null, aggregateRootId)) throw new ArgumentNullException(nameof(aggregateRootId));

            if (connection.IsConnected == false)
                return Result.Error($"Unreachable endpoint '{connection.ClientName}'.");

            var revisionKey = CreateRedisRevisionKey(aggregateRootId);

            try
            {
                var result = connection.GetDatabase().KeyExists(revisionKey);

                return new Result<bool>(result);
            }
            catch (Exception ex)
            {
                return Result.Error(ex);
            }
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        private string CreateRedisRevisionKey(IAggregateRootId aggregateRootId)
        {
            var stringRawId = Convert.ToBase64String(aggregateRootId.RawId);

            return string.Concat("revision-", stringRawId);
        }
    }
}
