using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class BuildingoutCompaniesforCloudTalent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoogleCloudUri",
                table: "Company",
                newName: "CloudTalentUri");

            migrationBuilder.AddColumn<string>(
                name: "CloudTalentIndexInfo",
                table: "Company",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CloudTalentIndexStatus",
                table: "Company",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsHiringAgency",
                table: "Company",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsJobPoster",
                table: "Company",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudTalentIndexInfo",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "CloudTalentIndexStatus",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "IsHiringAgency",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "IsJobPoster",
                table: "Company");

            migrationBuilder.RenameColumn(
                name: "CloudTalentUri",
                table: "Company",
                newName: "GoogleCloudUri");
        }
    }
}
