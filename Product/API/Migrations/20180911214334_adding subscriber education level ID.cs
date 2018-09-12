using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingsubscribereducationlevelID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelOfEducation",
                table: "Subscriber");

            migrationBuilder.AddColumn<int>(
                name: "EducationLevelID",
                table: "Subscriber",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevelID",
                table: "Subscriber");

            migrationBuilder.AddColumn<string>(
                name: "LevelOfEducation",
                table: "Subscriber",
                nullable: true);
        }
    }
}
