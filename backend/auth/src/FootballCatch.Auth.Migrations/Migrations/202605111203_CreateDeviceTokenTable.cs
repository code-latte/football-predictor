using FluentMigrator;

namespace FootballCatch.Auth.Migrations.Migrations;

[Migration(202605111203)]
public class M003_CreateDeviceTokenTable : Migration
{
    public override void Up()
    {
        Create.Table("auth_DeviceToken")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("UserId").AsGuid().NotNullable().ForeignKey("users", "Id").OnDelete(System.Data.Rule.Cascade)
            .WithColumn("DeviceIdentifier").AsString().NotNullable()
            .WithColumn("DeviceName").AsString(200).NotNullable()
            .WithColumn("Platform").AsString(50).NotNullable()
            .WithColumn("LastSeenAt").AsDateTime().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("RevokedAt").AsDateTime().Nullable();

        // Índice único compuesto UserId + DeviceIdentifier
        // (dos usuarios pueden registrarse desde el mismo dispositivo)
        Create.Index("IX_DeviceToken_UserId_DeviceIdentifier")
            .OnTable("DeviceToken")
            .OnColumn("UserId").Ascending()
            .OnColumn("DeviceIdentifier").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("auth_DeviceToken");
    }
}