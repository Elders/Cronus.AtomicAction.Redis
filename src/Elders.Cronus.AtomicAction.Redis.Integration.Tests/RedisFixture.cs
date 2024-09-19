using Testcontainers.Redis;

[SetUpFixture]
public class RedisFixture : IAsyncDisposable
{
    public static RedisContainer Container { get; private set; }

    [OneTimeSetUp]
    public async Task SetUp()
    {
        Container = new RedisBuilder()
            .Build();

        await Container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Container != null)
            await Container.DisposeAsync();
    }
}
