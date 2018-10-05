using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedhiddenfieldtocourseandtopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hidden",
                table: "Topic",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Hidden",
                table: "Course",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Course");
        }
    }
}
