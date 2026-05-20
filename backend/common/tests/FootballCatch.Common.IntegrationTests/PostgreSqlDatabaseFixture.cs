using Testcontainers.PostgreSql;
using FootballCatch.Common.IntegrationTests.Infrastructure;

namespace FootballCatch.Common.IntegrationTests;

[SetUpFixture]
public class PostgreSqlDatabaseFixture
{
    private static PostgreSqlContainer? _container;

    public static string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started. Did SetUpFixture run?");

    [OneTimeSetUp]
    public async Task BeforeAllTests()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("footballcatch")
            .WithUsername("testuser")
            .WithPassword("testpass!")
            .Build();

        await _container.StartAsync();
        MigrationsApplier.ApplyAll(ConnectionString);
    }

    [OneTimeTearDown]
    public async Task AfterAllTests()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
