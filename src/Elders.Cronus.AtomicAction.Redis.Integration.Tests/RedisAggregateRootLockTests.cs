using Elders.Cronus.AtomicAction.Redis.AggregateRootLock;
using Elders.RedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.AtomicAction.Redis.Integration.Tests;

[TestFixture]
public class RedisAggregateRootLockTests
{
    RedisAggregateRootLock rootLock;

    [SetUp]
    public void Setup()
    {
        var lockManager = new RedisLockManager(new RedisOptions(RedisFixture.Container.GetConnectionString()), new NullLoggerFactory().CreateLogger<RedisLockManager>());
        rootLock = new RedisAggregateRootLock(lockManager);
    }

    [Test]
    public async Task LockAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        var locked = await rootLock.LockAsync(resource, TimeSpan.FromSeconds(10));
        var result = await RedisFixture.Container.ExecScriptAsync($"return redis.call('GET', '{resource}')");

        Assert.That(locked, Is.True);
        Assert.That(string.IsNullOrWhiteSpace(result.Stdout), Is.False);
        Assert.That(string.IsNullOrWhiteSpace(result.Stderr), Is.True);
        Assert.That(result.ExitCode, Is.Zero);
    }

    [Test]
    public async Task IsLockedAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        await rootLock.LockAsync(resource, TimeSpan.FromSeconds(10));
        var locked = await rootLock.IsLockedAsync(resource);
        var result = await RedisFixture.Container.ExecScriptAsync($"return redis.call('GET', '{resource}')");

        Assert.That(locked, Is.True);
        Assert.That(string.IsNullOrWhiteSpace(result.Stdout), Is.False);
        Assert.That(string.IsNullOrWhiteSpace(result.Stderr), Is.True);
        Assert.That(result.ExitCode, Is.Zero);
    }

    [Test]
    public async Task UnlockAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        await rootLock.LockAsync(resource, TimeSpan.FromSeconds(10));
        await rootLock.UnlockAsync(resource);
        var result = await RedisFixture.Container.ExecScriptAsync($"return redis.call('EXISTS', '{resource}')");

        Assert.That(result.Stdout.Trim(), Is.EqualTo("0"));
        Assert.That(string.IsNullOrWhiteSpace(result.Stderr), Is.True);
        Assert.That(result.ExitCode, Is.Zero);
    }
}

file sealed class RedisOptions : IOptionsMonitor<RedLockOptions>, IDisposable
{
    private readonly string connectionString;

    public RedisOptions(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public RedLockOptions CurrentValue => new()
    {
        ConnectionString = connectionString
    };

    public void Dispose() { }

    public RedLockOptions Get(string name)
    {
        return CurrentValue;
    }

    public IDisposable OnChange(Action<RedLockOptions, string> listener)
    {
        listener(CurrentValue, null);
        return this;
    }
}
