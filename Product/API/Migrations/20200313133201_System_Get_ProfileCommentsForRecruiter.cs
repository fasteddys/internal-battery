using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class System_Get_ProfileCommentsForRecruiter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>     
2020.03.13 - Bill Koenig - Created
</remarks>
<description>
Returns profile comments. The subscriber guid is a security measure to ensure that comments cannot be viewed by people other than the person that created it 
(or people within the same company if the comment is visible to the company).
</description>
<example>
EXEC [G2].[System_Get_ProfileCommentsForRecruiter] @ProfileGuid = ''30881D3C-CA86-4D34-8D49-97B703625D72'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [G2].[System_Get_ProfileCommentsForRecruiter] (
	@ProfileGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT c.ProfileCommentId
    	FROM G2.ProfileComments c 
		INNER JOIN G2.Profiles p ON c.ProfileId = p.ProfileId
		LEFT JOIN dbo.Recruiter r ON c.RecruiterId = r.RecruiterId
		LEFT JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
    	WHERE c.IsDeleted = 0
		AND p.ProfileGuid = @ProfileGuid
		AND (s.SubscriberGuid = @SubscriberGuid OR c.IsVisibleToCompany = 1)
    )
    SELECT c.ProfileCommentGuid CommentGuid
		, p.ProfileGuid
        , r.RecruiterGuid
		, c.[Value]
		, c.IsVisibleToCompany
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM G2.ProfileComments c 
	INNER JOIN G2.Profiles p ON c.ProfileId = p.ProfileId
	LEFT JOIN dbo.Recruiter r ON c.RecruiterId = r.RecruiterId
	LEFT JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
    WHERE c.IsDeleted = 0
	AND p.ProfileGuid = @ProfileGuid
	AND (s.SubscriberGuid = @SubscriberGuid OR c.IsVisibleToCompany = 1)

    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN c.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [G2].[System_Get_ProfileCommentsForRecruiter]");
        }
    }
}
