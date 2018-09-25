using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedCourseCodetoStringinWozCourseSections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "WozCourseSection",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CourseCode",
                table: "WozCourseSection",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
