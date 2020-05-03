using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingconnecttedid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectedId",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectedModifyDate",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "ConnectedModifyDate",
                table: "Subscriber");
        }
    }
}
