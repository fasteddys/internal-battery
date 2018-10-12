using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedbasemodeltopromocode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "PromoCode",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "PromoCode",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "PromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "PromoCode",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "PromoCode",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "PromoCode");
        }
    }
}
