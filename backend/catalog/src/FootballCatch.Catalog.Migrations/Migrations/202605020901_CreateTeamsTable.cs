using FluentMigrator;

namespace FootballCatch.Catalog.Migrations.Migrations;

[Migration(202605020901)]
public class CreateTeamsTable : Migration
{
    public override void Up()
    {
        Create.Table("Teams")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("ShortCode").AsString(5).NotNullable()
            .WithColumn("CountryIsoCode").AsFixedLengthString(2).NotNullable()
            .WithColumn("CountryDisplayName").AsString(100).NotNullable()
            .WithColumn("CrestUrl").AsString(2048).Nullable()
            .WithColumn("ExternalId").AsString(100).Nullable()
            .WithColumn("CreatedAtUtc").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("UpdatedAtUtc").AsCustom("timestamp with time zone").NotNullable();

        Execute.Sql(
            "CREATE UNIQUE INDEX \"IX_Teams_ExternalId\" " +
            "ON \"Teams\" (\"ExternalId\") WHERE \"ExternalId\" IS NOT NULL");
    }

    public override void Down()
    {
        Delete.Table("Teams");
    }
}
