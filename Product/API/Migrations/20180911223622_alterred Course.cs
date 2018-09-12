using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class alterredCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Course",
                newName: "TabletImage");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "Course",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "CourseSchedule",
                table: "Course",
                newName: "TopicId");

            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "Course",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "CourseDeliveryId",
                table: "Course",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CourseDescription",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Course",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Course",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "DesktopImage",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileImage",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Course",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Course",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Course",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseDeliveryId",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CourseDescription",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "DesktopImage",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "MobileImage",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "Course",
                newName: "CourseSchedule");

            migrationBuilder.RenameColumn(
                name: "TabletImage",
                table: "Course",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Course",
                newName: "PurchasePrice");

            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "Course",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
