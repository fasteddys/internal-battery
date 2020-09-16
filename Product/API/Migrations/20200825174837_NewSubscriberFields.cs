using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class NewSubscriberFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectedUrl",
                table: "Subscriber",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentlyWorkingForCC",
                table: "Subscriber",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentlyWorkingForCCExpiration",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextCheckinDate",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CurrentlyWorkingForCC",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CurrentlyWorkingForCCExpiration",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "NextCheckinDate",
                table: "Subscriber");
        }
    }
}
