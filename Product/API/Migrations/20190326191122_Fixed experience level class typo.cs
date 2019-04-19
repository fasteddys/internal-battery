using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Fixedexperiencelevelclasstypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExperiecneLevelGuid",
                table: "ExperienceLevel",
                newName: "ExperienceLevelGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExperienceLevelGuid",
                table: "ExperienceLevel",
                newName: "ExperiecneLevelGuid");
        }
    }
}
