using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FootballCatch.Common.IntegrationTests.Infrastructure;

internal static class MigrationsApplier
{
    // Module short-names as they appear in the assembly name FootballCatch.{Name}.Migrations.
    // Kept in one place so adding a module is a one-line change.
    private static readonly string[] ModuleNames =
    [
        "Auth",
        "Catalog",
        "Fixtures",
        "Leagues",
        "Notifications",
        "Predictions",
        "ScoringEngine",
        "Stats",
        "Updater",
        "UserProfile",
    ];

    public static void ApplyAll(string connectionString)
    {
        // TODO: per-module VersionTableMetaData per ADR-0007. Default VersionInfo is fine for now (single throwaway container).
        var migrationAssemblies = ModuleNames
            .Select(name => Assembly.Load($"FootballCatch.{name}.Migrations"))
            .ToArray();

        var services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(migrationAssemblies).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}
