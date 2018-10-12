using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class changingenrollmentidtoenrollmentguid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentId",
                table: "WozCourseEnrollment");

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentGuid",
                table: "WozCourseEnrollment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentGuid",
                table: "WozCourseEnrollment");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentId",
                table: "WozCourseEnrollment",
                nullable: false,
                defaultValue: 0);
        }
    }
}
