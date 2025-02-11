using Elders.Cronus.AtomicAction.Redis.Config;
using Elders.Cronus.AtomicAction.Redis.RevisionStore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.AtomicAction.Redis.Integration.Tests;

[TestFixture]
public class RedisRevisionStoreTests
{
    RedisRevisionStore revisionStore;

    [SetUp]
    public void SetUp()
    {
        revisionStore = new RedisRevisionStore(new RevisionStoreOptions(RedisFixture.Container.GetConnectionString()), NullLogger<RedisRevisionStore>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        revisionStore?.Dispose();
    }

    [Test]
    public async Task PrepareRevisionAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        var result = await revisionStore.PrepareRevisionAsync(resource, 1);

        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task ConsecutivePrepareRevisionAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        await revisionStore.PrepareRevisionAsync(resource, 2);
        var result = await revisionStore.PrepareRevisionAsync(resource, 1);

        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task SaveRevisionAsync()
    {
        var resource = Guid.NewGuid().ToString("n");
        var result = await revisionStore.SaveRevisionAsync(resource, 1, TimeSpan.FromSeconds(20));

        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(result.Value, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }
}

file sealed class RevisionStoreOptions : IOptionsMonitor<RedisAtomicActionOptions>
{
    private readonly string connectionString;

    public RevisionStoreOptions(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public RedisAtomicActionOptions CurrentValue => new()
    {
        ConnectionString = connectionString
    };

    public RedisAtomicActionOptions Get(string name)
    {
        return CurrentValue;
    }

    public IDisposable OnChange(Action<RedisAtomicActionOptions, string> listener)
    {
        listener(CurrentValue, null);
        return null;
    }
}
