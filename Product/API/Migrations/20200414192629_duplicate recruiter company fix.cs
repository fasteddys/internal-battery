using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class duplicaterecruitercompanyfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>     
2020.03.17 - Bill Koenig - Created
2020.04.14 - Bill Koenig - Added DISTINCT to prevent duplicate skills from appearing if the recruiter has duplicate references to the same company
</remarks>
<description>
Returns profile skill details. The subscriber guid is a security measure to ensure that profiles cannot be viewed by people other than those associated with the profile''s company.
</description>
<example>
EXEC [G2].[System_Get_ProfileSkillsForRecruiter] @ProfileGuid = ''61134EB1-03F1-47C3-8D38-59776BEB9705'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 2, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [G2].[System_Get_ProfileSkillsForRecruiter] (
	@ProfileGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
	WITH profileSkills AS (
		SELECT DISTINCT ps.ProfileSkillGuid
			, p.ProfileGuid
			, k.SkillGuid
			, k.SkillName [Name]
			, ps.CreateDate
			, ps.ModifyDate
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
	)
	SELECT ProfileSkillGuid
		, ProfileGuid
		, SkillGuid
		, [Name]
    	, (SELECT COUNT(1) FROM profileSkills) TotalRecords
	FROM profileSkills
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
2020.04.14 - Bill Koenig - Added DISTINCT to prevent duplicate tags from appearing if the recruiter has duplicate references to the same company
</remarks>
<description>
Returns profile tags. The subscriber guid is a security measure to ensure that profile tags cannot be seen by recruiters from other companies.
</description>
<example>
EXEC [G2].[System_Get_ProfileTagsForRecruiter] @ProfileGuid = ''61134EB1-03F1-47C3-8D38-59776BEB9705'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [G2].[System_Get_ProfileTagsForRecruiter] (
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
		SELECT DISTINCT pt.ProfileTagGuid
			, t.TagGuid
			, p.ProfileGuid
			, t.[Name]
			, t.[Description]
			, pt.CreateDate
			, pt.ModifyDate
		FROM G2.ProfileTags pt 
		INNER JOIN dbo.Tag t ON pt.TagId = t.TagId
		INNER JOIN G2.Profiles p ON pt.ProfileId = p.ProfileId
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		INNER JOIN dbo.RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN dbo.Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN dbo.Subscriber s ON r.SubscriberId = s.SubscriberId
		WHERE pt.IsDeleted = 0 AND p.ProfileGuid = @ProfileGuid AND s.SubscriberGuid = @SubscriberGuid
	)
	SELECT ProfileTagGuid
		, TagGuid
		, ProfileGuid
		, [Name]
		, [Description]
		, (SELECT COUNT(1) FROM tags) TotalRecords
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

        }
    }
}
