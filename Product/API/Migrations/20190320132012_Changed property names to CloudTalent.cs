using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedpropertynamestoCloudTalent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoogleCloudUri",
                table: "JobPosting",
                newName: "CloudTalentUri");

            migrationBuilder.RenameColumn(
                name: "GoogleCloudIndexStatus",
                table: "JobPosting",
                newName: "CloudTalentIndexStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CloudTalentUri",
                table: "JobPosting",
                newName: "GoogleCloudUri");

            migrationBuilder.RenameColumn(
                name: "CloudTalentIndexStatus",
                table: "JobPosting",
                newName: "GoogleCloudIndexStatus");
        }
    }
}
