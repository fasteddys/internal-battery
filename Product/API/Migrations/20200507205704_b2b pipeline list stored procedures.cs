using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class b2bpipelineliststoredprocedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.05.07 - Bill Koenig - Created
</remarks>
<description>
Returns pipelines. The subscriber guid is a security measure to ensure that pipelines cannot be viewed by people other than the hiring manager who created it.
</description>
<example>
EXEC [B2B].[System_Get_PipelinesForHiringManager] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''createDate'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [B2B].[System_Get_PipelinesForHiringManager] (
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

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.05.06 - Bill Koenig - Created
</remarks>
<description>
Returns pipeline profiles details. The subscriber guid is a security measure to ensure that Pipelines cannot be viewed by people other than the hiring manager who created it.
</description>
<example>
EXEC [B2B].[System_Get_PipelineProfilesForHiringManager] @PipelineGuid = ''F99FF752-D858-47D4-BC23-533C463050C4'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''createDate'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [B2B].[System_Get_PipelineProfilesForHiringManager] (
	@PipelineGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN
    WITH allRecords AS (
        SELECT pp.PipelineProfileGuid
            , a_p.ProfileGuid
            , hm.HiringManagerGuid
			, a_s.Title
			, a_c.[Name] City
			, a_st.[Name] [State]
			, pp.CreateDate
			, pp.ModifyDate
        FROM dbo.Subscriber s
        INNER JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
		INNER JOIN B2B.Pipelines p ON hm.HiringManagerId = p.HiringManagerId
        LEFT JOIN B2B.PipelineProfiles pp ON p.PipelineId = pp.PipelineId
		LEFT JOIN G2.Profiles a_p ON pp.ProfileId = a_p.ProfileId
		LEFT JOIN dbo.Subscriber a_s ON a_p.SubscriberId = a_s.SubscriberId
		LEFT JOIN dbo.City a_c ON a_s.CityGuid = a_c.CityGuid
        LEFT JOIN dbo.[State] a_st ON a_s.StateGuid = a_st.StateGuid
        WHERE s.IsDeleted = 0
		AND p.IsDeleted = 0
		AND a_p.IsDeleted = 0
		AND hm.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
		AND p.PipelineGuid = @PipelineGuid)
    SELECT PipelineProfileGuid
		, ProfileGuid
        , HiringManagerGuid
        , City
        , [State]
        , Title
        , (SELECT COUNT(1) FROM allRecords) AS TotalRecords
    FROM allRecords
    ORDER BY  
        CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN Title END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''city'' THEN City END,
		CASE WHEN @Order = ''ascending'' AND @Sort = ''state'' THEN [State] END, 
		CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
		CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
        CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN Title END DESC,
        CASE WHEN @Order = ''descending'' AND @Sort = ''city'' THEN City END DESC,
        CASE WHEN @Order = ''descending'' AND @Sort = ''state'' THEN [State] END DESC,		
		CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
		CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
