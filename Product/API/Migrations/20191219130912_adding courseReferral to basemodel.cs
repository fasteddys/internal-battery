using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingcourseReferraltobasemodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "CourseReferral",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "CourseReferral",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "CourseReferral",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "CourseReferral",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "CourseReferral",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "CourseReferral");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "CourseReferral");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CourseReferral");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "CourseReferral");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "CourseReferral");
        }
    }
}
