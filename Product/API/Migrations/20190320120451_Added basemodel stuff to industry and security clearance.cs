using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addedbasemodelstufftoindustryandsecurityclearance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SecurityClearance",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "SecurityClearance",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "SecurityClearance",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "SecurityClearance",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "SecurityClearance",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Industry",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Industry",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "Industry",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Industry",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Industry",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SecurityClearance");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "SecurityClearance");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SecurityClearance");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "SecurityClearance");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "SecurityClearance");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Industry");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Industry");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Industry");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Industry");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Industry");
        }
    }
}
