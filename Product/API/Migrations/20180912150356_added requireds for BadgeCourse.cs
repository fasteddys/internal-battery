using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforBadgeCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BadgeID",
                table: "BadgeCourse",
                newName: "CourseID");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "BadgeCourse",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeCourseGuid",
                table: "BadgeCourse",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "BadgeCourse",
                newName: "BadgeID");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "BadgeCourse",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeCourseGuid",
                table: "BadgeCourse",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
