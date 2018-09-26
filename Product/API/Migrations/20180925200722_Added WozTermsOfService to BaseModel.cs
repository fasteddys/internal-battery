using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedWozTermsOfServicetoBaseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "WozTermsOfService",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "WozTermsOfService",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "WozTermsOfService",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "WozTermsOfService",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "WozTermsOfService",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "WozTermsOfService");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "WozTermsOfService");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WozTermsOfService");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "WozTermsOfService");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "WozTermsOfService");
        }
    }
}
