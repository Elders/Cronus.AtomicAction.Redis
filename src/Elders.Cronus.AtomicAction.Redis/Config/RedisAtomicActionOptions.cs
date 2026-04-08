using System;
using Elders.RedLock;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAtomicActionOptions
    {
        /// <summary>
        /// Gets or sets the connection string used to establish a connection to the database.
        /// This is an optional property. If not provided, the ConnectionName property will be used to get the connection string from ConnectionStrings configuration.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the connection string to use.
        /// By default it is "redis".
        /// </summary>
        public string ConnectionName { get; set; } = "redis";

        /// <summary>
        /// The TTL which is applied in the beginning of the execution of the atomic action.
        /// By default it is 1 second.
        /// </summary>
        public TimeSpan LockTtl { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// This TTL is applied after a successful execution of the atomic action.
        /// The reason behind this decision is to make sure that there are no other nodes/threads which
        /// are executing an action against the specific AR + revision. If we do not do this there is a
        /// chance some other node/thread to overwrite our last action. By default this lock lasts for
        /// 5 seconds and it does not interrupt any other operations over the AR in normal action flow.
        /// </summary>
        public TimeSpan LongTtl { get; set; } = TimeSpan.FromSeconds(5);
    }

    internal class RedisAtomicActionOptionsProvider : CronusOptionsProviderBase<RedisAtomicActionOptions>
    {
        public RedisAtomicActionOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(RedisAtomicActionOptions options)
        {
            configuration.GetSection("cronus:atomicaction:redis").Bind(options);
            
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                var connectionName = string.IsNullOrEmpty(options.ConnectionName) ? "redis" : options.ConnectionName;
                var aspireConnectionString = configuration.GetConnectionString(connectionName);
                if (!string.IsNullOrEmpty(aspireConnectionString))
                {
                    options.ConnectionString = aspireConnectionString;
                }
            }
        }
    }

    internal class AtomicActionRedLockOptionsProvider : CronusOptionsProviderBase<RedLockOptions>
    {
        public AtomicActionRedLockOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(RedLockOptions options)
        {
            configuration.GetSection("cronus:atomicaction:redis").Bind(options);
            
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                var redisOptions = new RedisAtomicActionOptions();
                configuration.GetSection("cronus:atomicaction:redis").Bind(redisOptions);
                
                var connectionName = string.IsNullOrEmpty(redisOptions.ConnectionName) ? "redis" : redisOptions.ConnectionName;
                var aspireConnectionString = configuration.GetConnectionString(connectionName);
                if (!string.IsNullOrEmpty(aspireConnectionString))
                {
                    options.ConnectionString = aspireConnectionString;
                }
            }
        }
    }
}
