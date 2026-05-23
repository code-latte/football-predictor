using FluentMigrator;

namespace FootballCatch.Auth.Migrations.Migrations;

[Migration(202605111201)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("auth_users")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Email").AsString(200).NotNullable().Unique()
            .WithColumn("FullName").AsString(100).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("auth_users");
    }
}