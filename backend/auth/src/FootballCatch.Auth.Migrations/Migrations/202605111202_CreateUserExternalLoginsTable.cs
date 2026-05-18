
using FluentMigrator;
namespace FootballCatch.Auth.Migrations.Migrations;
[Migration(202605111202)]
public class CreateUserExternalLoginsTable : Migration
{
     public override void Up()
    {
        Create.Table("user_external_logins")
            .WithColumn("Provider").AsString(50).NotNullable()
            .WithColumn("ProviderUserId").AsString(200).NotNullable()
            .WithColumn("UserId").AsGuid().NotNullable().ForeignKey("users", "Id").OnDelete(System.Data.Rule.Cascade)
            .WithColumn("ProviderEmail").AsString(200).Nullable()
            .WithColumn("LinkedAt").AsDateTime().NotNullable();

        // Clave primaria compuesta
        Create.PrimaryKey("PK_user_external_logins")
            .OnTable("user_external_logins")
            .Columns("Provider", "ProviderUserId");

        // Índice único para Provider + ProviderUserId (equivalente al HasIndex del OwnsMany)
        Create.Index("IX_user_external_logins_Provider_ProviderUserId")
            .OnTable("user_external_logins")
            .OnColumn("Provider").Ascending()
            .OnColumn("ProviderUserId").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("user_external_logins");
    }
}
