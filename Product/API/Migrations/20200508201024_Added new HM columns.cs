using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddednewHMcolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "B2B");

            migrationBuilder.AddColumn<string>(
                name: "HardToFindFillSkillsRoles",
                schema: "B2B",
                table: "HiringManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillsRolesWeAreAlwaysHiringFor",
                schema: "B2B",
                table: "HiringManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HardToFindFillSkillsRoles",
                schema: "B2B",
                table: "HiringManagers");

            migrationBuilder.DropColumn(
                name: "SkillsRolesWeAreAlwaysHiringFor",
                schema: "B2B",
                table: "HiringManagers");

        }
    }
}
