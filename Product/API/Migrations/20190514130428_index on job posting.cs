using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class indexonjobposting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_IsDeletedJobPostingGuid",
                table: "JobPosting",
                columns: new[] { "IsDeleted", "JobPostingGuid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPosting_IsDeletedJobPostingGuid",
                table: "JobPosting");
        }
    }
}
