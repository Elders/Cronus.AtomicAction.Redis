using System;
using System.ComponentModel.DataAnnotations;
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

        public int LockRetryCount { get; set; } = 3;

        public TimeSpan LockRetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        public double ClockDriveFactor { get; set; } = 0.01;
    }

    public class RedisAtomicActionOptionsProvider : CronusOptionsProviderBase<RedisAtomicActionOptions>
    {
        public RedisAtomicActionOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(RedisAtomicActionOptions options)
        {
            configuration.GetSection("cronus:atomicaction:redis").Bind(options);
        }
    }
}
