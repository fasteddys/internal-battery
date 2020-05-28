using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class pipelinecounthotfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.05.07 - Bill Koenig - Created
2020.05.26 - JAB - Modified to return number of profiles in each pipeline 
2020.05.26 - JAB - Fixed self correlation error 
</remarks>
<description>
Returns pipelines. The subscriber guid is a security measure to ensure that pipelines cannot be viewed by people other than the hiring manager who created it.
</description>
<example>
EXEC [B2B].[System_Get_PipelinesForHiringManager] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''createDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [B2B].[System_Get_PipelinesForHiringManager] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT p.PipelineGuid 
			, hm.HiringManagerGuid
			, p.[Name]
			, p.[Description]
			, p.CreateDate
			, p.ModifyDate
			, p.PipelineId
    	FROM B2B.Pipelines p
		INNER JOIN B2B.HiringManagers hm ON p.HiringManagerId = hm.HiringManagerId
		INNER JOIN Subscriber s ON hm.SubscriberId = s.SubscriberId
    	WHERE p.IsDeleted = 0
		AND hm.IsDeleted = 0
		AND s.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
    )
    SELECT PipelineGuid	    
		, HiringManagerGuid
		, [Name]
		, [Description]
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
		, (SELECT COUNT(1) FROM b2b.PipelineProfiles pp WHERE pp.PipelineId = allRecords.PipelineId and pp.IsDeleted = 0) [ProfileCount] 
    FROM allRecords
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.05.07 - Bill Koenig - Created
2020.05.26 - JAB - Modified to return number of profiles in each pipeline 
</remarks>
<description>
Returns pipelines. The subscriber guid is a security measure to ensure that pipelines cannot be viewed by people other than the hiring manager who created it.
</description>
<example>
EXEC [B2B].[System_Get_PipelinesForHiringManager] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''createDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [B2B].[System_Get_PipelinesForHiringManager] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT p.PipelineGuid 
			, hm.HiringManagerGuid
			, p.[Name]
			, p.[Description]
			, p.CreateDate
			, p.ModifyDate
			, p.PipelineId
    	FROM B2B.Pipelines p
		INNER JOIN B2B.HiringManagers hm ON p.HiringManagerId = hm.HiringManagerId
		INNER JOIN Subscriber s ON hm.SubscriberId = s.SubscriberId
    	WHERE p.IsDeleted = 0
		AND hm.IsDeleted = 0
		AND s.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
    )
    SELECT PipelineGuid	    
		, HiringManagerGuid
		, [Name]
		, [Description]
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
		, (SELECT COUNT(1) FROM b2b.PipelineProfiles pp WHERE pp.PipelineId = PipelineId and pp.IsDeleted = 0) [ProfileCount] 
    FROM allRecords
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

        }
    }
}
