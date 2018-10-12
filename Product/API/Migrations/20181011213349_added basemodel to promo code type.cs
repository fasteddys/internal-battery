using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedbasemodeltopromocodetype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "PromoType",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "PromoType",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "PromoType",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "PromoType",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "PromoType",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "PromoType");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "PromoType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PromoType");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "PromoType");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "PromoType");
        }
    }
}
