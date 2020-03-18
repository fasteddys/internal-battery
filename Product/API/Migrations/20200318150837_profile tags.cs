using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class profiletags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.18 - Bill Koenig - Created
</remarks>
<description>
Retrieves tags
</description>
<example>
EXEC [dbo].[System_Get_Tags] @Limit = 4, @Offset = 0, @Sort = ''name'', @Order = ''descending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Tags] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT TagId
		FROM Tag 		
		WHERE IsDeleted = 0
	)
    SELECT TagGuid
        , [Name]
		, [Description]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Tag
    WHERE IsDeleted = 0
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
2020.03.18 - Bill Koenig - Created
</remarks>
<description>
Returns profile tags. The subscriber guid is a security measure to ensure that profile tags cannot be seen by recruiters from other companies.
</description>
<example>
EXEC [G2].[System_Get_ProfileTagsForRecruiter] @ProfileGuid = ''1ECA1D45-B6E6-4A04-92CA-F45FA7064AC5'', @SubscriberGuid = ''CEE8B3A2-FED0-4EBA-AFF9-E39D2630E5C2'', @Limit = 10, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [G2].[System_Get_ProfileTagsForRecruiter] (
	@ProfileGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
	WITH tags AS (
		SELECT pt.ProfileTagGuid, t.TagGuid, p.ProfileGuid, t.[Name], t.[Description], pt.CreateDate, pt.ModifyDate
		FROM G2.ProfileTags pt 
		INNER JOIN dbo.Tag t ON pt.TagId = t.TagId
		INNER JOIN G2.Profiles p ON pt.ProfileId = p.ProfileId
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		INNER JOIN dbo.RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN dbo.Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN dbo.Subscriber s ON r.SubscriberId = s.SubscriberId
		WHERE pt.IsDeleted = 0 AND p.ProfileGuid = @ProfileGuid AND s.SubscriberGuid = @SubscriberGuid
	)
	SELECT *, (SELECT COUNT(1) FROM tags) [TotalRecords]
	FROM tags
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
            migrationBuilder.Sql("DROP PROCEDURE [G2].[System_Get_ProfileTagsForRecruiter]");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_Tags]");
        }
    }
}
