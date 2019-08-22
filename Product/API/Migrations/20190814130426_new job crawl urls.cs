using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class newjobcrawlurls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE dbo.JobSite
SET Uri = 'https://www.teksystems.com/it-jobs/en-US/search', 
	ModifyDate = GETUTCDATE(),
	IsDeleted = 0
WHERE [Name] = 'TEKsystems'

UPDATE dbo.JobSite
SET Uri = 'https://www.aerotek.com/jobs/en-US/search', 
	ModifyDate = GETUTCDATE(),
	IsDeleted = 0
WHERE [Name] = 'Aerotek'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE dbo.JobSite
SET Uri = 'https://allegis-tek.cbtalentnetwork.com/it-jobs/api/results?geoIp=false', 
	ModifyDate = GETUTCDATE(),
	IsDeleted = 0
WHERE [Name] = 'TEKsystems'

UPDATE dbo.JobSite
SET Uri = 'https://allegis-aerotek.cbtalentnetwork.com/jobs/api/results?geoIp=false', 
	ModifyDate = GETUTCDATE(),
	IsDeleted = 0
WHERE [Name] = 'Aerotek'
            ");
        }
    }
}
