using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class InheritVendorStudentLoginfromBaseMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "VendorStudentLogin",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "VendorStudentLogin",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "VendorStudentLogin",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "VendorStudentLogin",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "VendorStudentLogin",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "VendorStudentLogin");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "VendorStudentLogin");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VendorStudentLogin");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "VendorStudentLogin");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "VendorStudentLogin");
        }
    }
}
