using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modificationstojobsiteandjobsitescrapestatistic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumberOfWebRequestsMade",
                table: "JobSiteScrapeStatistic",
                newName: "UnsuccessfulWebRequests");

            migrationBuilder.AddColumn<int>(
                name: "SuccessfulWebRequests",
                table: "JobSiteScrapeStatistic",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "JobSite",
                nullable: true);

            migrationBuilder.Sql("UPDATE dbo.JobSite SET MaxRetries = 3, ModifyDate = GETUTCDATE()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuccessfulWebRequests",
                table: "JobSiteScrapeStatistic");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "JobSite");

            migrationBuilder.RenameColumn(
                name: "UnsuccessfulWebRequests",
                table: "JobSiteScrapeStatistic",
                newName: "NumberOfWebRequestsMade");
        }
    }
}
