
using FluentMigrator;
namespace FootballCatch.Auth.Migrations.Migrations;

[Migration(202605271300)]
public class CreateOtpCodeTable : Migration
{
    public override void Up()
    {
        Create.Table("otp_codes")  
            .WithColumn("uid").AsGuid().NotNullable()
            .WithColumn("user_email").AsString(200).NotNullable()
            .WithColumn("hash").AsString(200).NotNullable()
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("used_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable();

        Create.PrimaryKey("pk_otp_codes")
            .OnTable("otp_codes")
            .Column("uid");

        Create.Index("ix_otp_codes_user_email")
            .OnTable("otp_codes")
            .OnColumn("user_email");
    }
    public override void Down()
    {
        Delete.Table("otp_codes");
    }  
}