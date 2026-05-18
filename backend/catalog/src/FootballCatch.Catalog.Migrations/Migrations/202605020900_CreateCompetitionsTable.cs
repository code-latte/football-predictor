using FluentMigrator;

namespace FootballCatch.Catalog.Migrations.Migrations;

[Migration(202605020900)]
public class CreateCompetitionsTable : Migration
{
    public override void Up()
    {
        Create.Table("catalog_competitions")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("CountryIsoCode").AsFixedLengthString(2).NotNullable()
            .WithColumn("CountryDisplayName").AsString(100).NotNullable()
            .WithColumn("SeasonStartYear").AsInt32().NotNullable()
            .WithColumn("SeasonEndYear").AsInt32().NotNullable()
            .WithColumn("LogoUrl").AsString(2048).Nullable()
            .WithColumn("ExternalId").AsString(100).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable()
            .WithColumn("CreatedAtUtc").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("UpdatedAtUtc").AsCustom("timestamp with time zone").NotNullable();

        Execute.Sql(
            "CREATE UNIQUE INDEX \"ix_catalog_competitions_external_id\" " +
            "ON \"catalog_competitions\" (\"ExternalId\") WHERE \"ExternalId\" IS NOT NULL");
    }

    public override void Down()
    {
        Delete.Table("catalog_competitions");
    }
}
