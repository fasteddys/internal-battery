using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class additionaljobscrapetrackingforwebproxy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfWebRequestsMade",
                table: "JobSiteScrapeStatistic",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TotalResponseSizeInBytes",
                table: "JobSiteScrapeStatistic",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfWebRequestsMade",
                table: "JobSiteScrapeStatistic");

            migrationBuilder.DropColumn(
                name: "TotalResponseSizeInBytes",
                table: "JobSiteScrapeStatistic");
        }
    }
}
