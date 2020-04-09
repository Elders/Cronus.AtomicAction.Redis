using System;
using System.ComponentModel.DataAnnotations;
using Elders.RedLock;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.AtomicAction.Redis.Config
{
    public class RedisAtomicActionOptions
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The configuration `Cronus:AtomicAction:Eedis:ConnectionString` is required. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md")]
        public string ConnectionString { get; set; }

        public TimeSpan LockTtl { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan ShorTtl { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan LongTtl { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class RedisAtomicActionOptionsProvider : CronusOptionsProviderBase<RedisAtomicActionOptions>
    {
        public RedisAtomicActionOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(RedisAtomicActionOptions options)
        {
            configuration.GetSection("cronus:atomicaction:redis").Bind(options);
        }
    }

    public class RedLockOptionsProvider : CronusOptionsProviderBase<RedLockOptions>
    {
        public RedLockOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(RedLockOptions options)
        {
            configuration.GetSection("cronus:atomicaction:redis").Bind(options);
        }
    }
}
