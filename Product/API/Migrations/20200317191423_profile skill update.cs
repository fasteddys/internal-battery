using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class profileskillupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>     
2020.03.17 - Bill Koenig - Created
</remarks>
<description>
Returns profile skill details. The subscriber guid is a security measure to ensure that profiles cannot be viewed by people other than those associated with the profile''s company.
</description>
<example>
EXEC [G2].[System_Get_ProfileSkillsForRecruiter] @ProfileGuid = ''30881D3C-CA86-4D34-8D49-97B703625D72'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [G2].[System_Get_ProfileSkillsForRecruiter] (
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
    	SELECT ps.ProfileSkillId
    	FROM G2.ProfileSkills ps
		INNER JOIN G2.Profiles p ON ps.ProfileId = p.ProfileId	
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		INNER JOIN dbo.RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN dbo.Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
    	WHERE ps.IsDeleted = 0
		AND p.ProfileGuid = @ProfileGuid
		AND s.SubscriberGuid = @SubscriberGuid
    )
    SELECT ps.ProfileSkillGuid
		, p.ProfileGuid
		, k.SkillGuid
		, k.SkillName [Name]
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    	FROM G2.ProfileSkills ps
		INNER JOIN dbo.Skill k ON ps.SkillId = k.SkillId
		INNER JOIN G2.Profiles p ON ps.ProfileId = p.ProfileId	
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		INNER JOIN dbo.RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN dbo.Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
    WHERE ps.IsDeleted = 0
	AND p.ProfileGuid = @ProfileGuid
	AND s.SubscriberGuid = @SubscriberGuid
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN k.SkillName END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN ps.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ps.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN k.SkillName END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN ps.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ps.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [G2].[System_Get_ProfileSkillsForRecruiter]");
        }
    }
}
