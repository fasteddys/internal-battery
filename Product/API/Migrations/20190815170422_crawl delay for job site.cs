using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class crawldelayforjobsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CrawlDelayInMilliseconds",
                table: "JobSite",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE dbo.JobSite
SET CrawlDelayInMilliseconds = 5000, 
	ModifyDate = GETUTCDATE()
WHERE [Name] = 'TEKsystems'

UPDATE dbo.JobSite
SET CrawlDelayInMilliseconds = 3000, 
	ModifyDate = GETUTCDATE()
WHERE [Name] = 'Aerotek'
            ");
    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrawlDelayInMilliseconds",
                table: "JobSite");
        }
    }
}
