using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedvendor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Vendor",
                newName: "VendorName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Vendor",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Vendor",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorGuid",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "VendorGuid",
                table: "Vendor");

            migrationBuilder.RenameColumn(
                name: "VendorName",
                table: "Vendor",
                newName: "Name");
        }
    }
}
