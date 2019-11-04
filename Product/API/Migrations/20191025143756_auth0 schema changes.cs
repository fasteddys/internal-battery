using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class auth0schemachanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "Subscriber",
                newName: "IsEmailVerifiedLegacy");

            migrationBuilder.AddColumn<string>(
                name: "Auth0UserId",
                table: "Subscriber",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignIn",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Auth0UserId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "LastSignIn",
                table: "Subscriber");

            migrationBuilder.RenameColumn(
                name: "IsEmailVerifiedLegacy",
                table: "Subscriber",
                newName: "IsVerified");
        }
    }
}
