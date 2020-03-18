using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class g2securitymeasures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-03-14 - Bill Koenig - Created
</remarks>
<description>
Returns TRUE if the recruiter is in the same company as the profile. Otherwise, returns FALSE.
</description>
<example>
SELECT [dbo].[fn_IsRecruiterInSameCompanyAsProfile](6688, 502) --returns FALSE
SELECT [dbo].[fn_IsRecruiterInSameCompanyAsProfile](6688, 379) --returns TRUE
</example>
*/
CREATE FUNCTION [dbo].[fn_IsRecruiterInSameCompanyAsProfile](@RecruiterId INT, @ProfileId INT)
RETURNS BIT
AS
BEGIN

	IF EXISTS (
		SELECT rc.RecruiterCompanyId
		FROM dbo.Recruiter r 
		INNER JOIN dbo.RecruiterCompany rc ON r.RecruiterId = rc.RecruiterId
		INNER JOIN G2.Profiles p ON rc.CompanyId = p.CompanyId
		WHERE p.ProfileId = @ProfileId AND r.RecruiterId = @RecruiterId
		AND rc.IsDeleted = 0)
	BEGIN
		RETURN 1;
	END

	RETURN 0;
END')");

            migrationBuilder.Sql("ALTER TABLE [G2].[ProfileComments] WITH CHECK ADD CONSTRAINT [CK_Profiles_IsRecruiterInSameCompanyAsProfile] CHECK (([dbo].[fn_IsRecruiterInSameCompanyAsProfile]([RecruiterId], [ProfileId])=(1)))");

            migrationBuilder.Sql("ALTER TABLE [G2].[ProfileComments] CHECK CONSTRAINT [CK_Profiles_IsRecruiterInSameCompanyAsProfile]");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.03.13 - Bill Koenig - Created
20202.03.14 - Bill Koenig - Updated to properly enforce security requirements
</remarks>
<description>
Returns profile comments. The subscriber guid is a security measure to ensure that comments cannot be viewed by people other than the person that created it 
(or people within the same company if the comment is visible to the company).
</description>
<example>
EXEC [G2].[System_Get_ProfileCommentsForRecruiter] @ProfileGuid = ''30881D3C-CA86-4D34-8D49-97B703625D72'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [G2].[System_Get_ProfileCommentsForRecruiter] (
	@ProfileGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
	WITH recruiterInProfileCompany AS (
		-- logic to ensure the recruiter is within the company associated with the profile comments
		SELECT p.ProfileId
		FROM dbo.Subscriber s
		INNER JOIN dbo.Recruiter r ON s.SubscriberId = r.SubscriberId
		INNER JOIN dbo.RecruiterCompany rc ON r.RecruiterId = rc.RecruiterId
		INNER JOIN G2.Profiles p ON rc.CompanyId = p.CompanyId
		WHERE s.IsDeleted = 0 AND r.IsDeleted = 0 AND rc.IsDeleted = 0 AND p.IsDeleted = 0
		AND p.ProfileGuid = @ProfileGuid AND s.SubscriberGuid = @SubscriberGuid
	), recruiterComments AS (
		-- profile comments made by the recruiter making the request for profile comments
		SELECT pc.ProfileCommentGuid CommentGuid, p.ProfileGuid, r.RecruiterGuid, pc.[Value], pc.IsVisibleToCompany, pc.CreateDate, pc.ModifyDate
		FROM G2.ProfileComments pc 
		INNER JOIN G2.Profiles p ON pc.ProfileId = p.ProfileId
		INNER JOIN dbo.Recruiter r ON pc.RecruiterId = r.RecruiterId
		INNER JOIN dbo.Subscriber s ON r.SubscriberId = s.SubscriberId
		INNER JOIN recruiterInProfileCompany ripc ON p.ProfileId = ripc.ProfileId
		WHERE pc.IsDeleted = 0 AND r.IsDeleted = 0 AND p.IsDeleted = 0
		AND p.ProfileGuid = @ProfileGuid AND s.SubscriberGuid = @SubscriberGuid
	), otherComments AS (
		-- company-wide comments made by other recruiters
		SELECT pc.ProfileCommentGuid CommentGuid, p.ProfileGuid, r.RecruiterGuid, pc.[Value], pc.IsVisibleToCompany, pc.CreateDate, pc.ModifyDate
		FROM G2.ProfileComments pc
		INNER JOIN G2.Profiles p ON pc.ProfileId = p.ProfileId
		INNER JOIN dbo.Recruiter r ON pc.RecruiterId = r.RecruiterId
		INNER JOIN dbo.Subscriber s ON r.SubscriberId = s.SubscriberId
		INNER JOIN recruiterInProfileCompany ripc ON p.ProfileId = ripc.ProfileId
		WHERE pc.IsDeleted = 0 AND p.IsDeleted = 0 AND r.IsDeleted = 0 
		AND p.ProfileGuid = @ProfileGuid AND s.SubscriberGuid <> @SubscriberGuid AND pc.IsVisibleToCompany = 1
	), allComments AS (
		SELECT *
		FROM recruiterComments rc
		UNION
		SELECT *
		FROM otherComments oc
	)
	SELECT *, (SELECT COUNT(1) FROM allComments) [TotalRecords]
	FROM allComments
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.DropIndex(
                name: "IX_RecruiterCompany_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.CreateIndex(
                name: "UIX_RecruiterCompany_Recruiter_Company",
                table: "RecruiterCompany",
                columns: new[] { "CompanyId", "RecruiterId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE[G2].[ProfileComments] DROP CONSTRAINT [CK_Profiles_IsRecruiterInSameCompanyAsProfile]");

            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_IsRecruiterInSameCompanyAsProfile]");

            migrationBuilder.DropIndex(
                name: "UIX_RecruiterCompany_Recruiter_Company",
                table: "RecruiterCompany");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterCompany_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId");
        }
    }
}
