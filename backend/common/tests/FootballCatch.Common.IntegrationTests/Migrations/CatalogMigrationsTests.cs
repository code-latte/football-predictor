using FluentAssertions;
using Npgsql;

namespace FootballCatch.Common.IntegrationTests.Migrations;

[TestFixture]
public class CatalogMigrationsTests
{
    private static readonly string[] ExpectedCatalogTables =
    [
        "catalog_competitions",
        "catalog_teams",
    ];

    [Test]
    public async Task After_Migrations_All_Catalog_Tables_Exist()
    {
        var publicTables = await ReadPublicTablesAsync(PostgreSqlDatabaseFixture.ConnectionString);

        publicTables.Should().Contain(ExpectedCatalogTables);
    }

    private static async Task<HashSet<string>> ReadPublicTablesAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'",
            conn);

        var names = new HashSet<string>(StringComparer.Ordinal);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }
        return names;
    }
}
