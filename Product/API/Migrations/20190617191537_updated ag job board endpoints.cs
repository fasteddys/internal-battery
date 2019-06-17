using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedagjobboardendpoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE jp
SET Uri = CASE 
		WHEN js.[Name] = 'TEKsystems' THEN REPLACE(jp.Uri, 'https://www.teksystems.com/', 'https://allegis-tek.cbtalentnetwork.com/') 
		WHEN js.[Name] = 'Aerotek' THEN REPLACE(jp.Uri, 'https://www.aerotek.com/', 'https://allegis-aerotek.cbtalentnetwork.com/') 
	END
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM dbo.JobPage jp
INNER JOIN dbo.JobSite js on jp.JobSiteId = js.JobSiteId
            ");

            migrationBuilder.Sql(@"
UPDATE dbo.JobSite
SET Uri = 'https://allegis-aerotek.cbtalentnetwork.com/jobs/api/results?geoIp=false'
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
	, IsDeleted = 0
WHERE [Name] = 'Aerotek'

UPDATE dbo.JobSite
SET Uri = 'https://allegis-tek.cbtalentnetwork.com/it-jobs/api/results?geoIp=false'
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
	, IsDeleted = 0
WHERE [Name] = 'TEKsystems'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE jp
SET Uri = CASE 
		WHEN js.[Name] = 'TEKsystems' THEN REPLACE(jp.Uri, 'https://allegis-tek.cbtalentnetwork.com/', 'https://www.teksystems.com/') 
		WHEN js.[Name] = 'Aerotek' THEN REPLACE(jp.Uri, 'https://allegis-aerotek.cbtalentnetwork.com/', 'https://www.aerotek.com/') 
	END
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM dbo.JobPage jp
INNER JOIN dbo.JobSite js on jp.JobSiteId = js.JobSiteId
            ");

            migrationBuilder.Sql(@"
UPDATE dbo.JobSite
SET Uri = 'https://www.aerotek.com/jobs/api/results?geoIp=false'
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
	, IsDeleted = 0
WHERE [Name] = 'Aerotek'

UPDATE dbo.JobSite
SET Uri = 'https://www.teksystems.com/it-jobs/api/results?geoIp=false'
	, ModifyDate = GETUTCDATE()
	, ModifyGuid = '00000000-0000-0000-0000-000000000000'
	, IsDeleted = 0
WHERE [Name] = 'TEKsystems'
            ");
        }
    }
}
