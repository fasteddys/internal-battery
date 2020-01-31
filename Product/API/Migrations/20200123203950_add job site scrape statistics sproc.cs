using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addjobsitescrapestatisticssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.23 - Bill Koenig - Created
</remarks>
<description>
Retrieves job site scrape statistics
</description>
<example>
EXEC [dbo].[System_Get_JobSiteScrapeStatistics] @Limit = 10, @Offset = 0, @Sort = ''endDate'', @Order = ''descending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_JobSiteScrapeStatistics] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT JobSiteScrapeStatisticId
    	FROM JobSiteScrapeStatistic jsss
		INNER JOIN JobSite js ON jsss.JobSiteId = js.JobSiteId
    	WHERE jsss.IsDeleted = 0
    )
	SELECT js.Name [JobSite]
		, jsss.CreateDate
		, ScrapeDate [EndDate]
		, NumJobsAdded
		, NumJobsUpdated
		, NumJobsDropped
		, NumJobsErrored
		, NumJobsProcessed
		, ABS(DATEDIFF(MINUTE, jsss.CreateDate, ScrapeDate)) [MinutesElapsed]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM JobSiteScrapeStatistic jsss
	INNER JOIN JobSite js ON jsss.JobSiteId = js.JobSiteId
	WHERE jsss.IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''jobSite'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''endDate'' THEN jsss.ScrapeDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN jsss.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN jsss.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''jobSite'' THEN [Name]  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''endDate'' THEN jsss.ScrapeDate END DESC, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN jsss.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN jsss.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
