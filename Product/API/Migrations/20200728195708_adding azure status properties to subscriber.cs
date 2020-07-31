using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingazurestatuspropertiestosubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AzureIndexModifyDate",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AzureIndexStatusId",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureSearchIndexInfo",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureIndexModifyDate",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "AzureIndexStatusId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "AzureSearchIndexInfo",
                table: "Subscriber");
        }
    }
}
