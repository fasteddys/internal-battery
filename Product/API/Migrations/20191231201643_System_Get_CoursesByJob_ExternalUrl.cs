using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class System_Get_CoursesByJob_ExternalUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
			
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
2019.12.19 - Bill Koenig - Modifying image properties
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesByJob] @JobPostingGuid = ''52CEFB9E-0AD8-414E-8F48-A6709DBE2B0E'', @Limit = 5, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByJob] (
	@JobPostingGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	
	;WITH jobSkills AS (
		-- skills that are associated with the subscriber
		SELECT jps.SkillId
		FROM JobPosting j
		INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON jps.JobPostingId = j.JobPostingId
		WHERE j.JobPostingGuid = @JobPostingGuid
		AND jps.IsDeleted = 0)
	, jobSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
		FROM jobSkills js WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId
		UNION 
		SELECT js.SkillId, ISNULL(rjsm.PopularityIndex, 10)
		FROM jobSkills js
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, MIN(SkillScore) HighestSkillScore
		FROM jobSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT cs.CourseId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		WHERE cs.IsDeleted = 0
		GROUP BY cs.CourseId)
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, c.ExternalUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	INNER JOIN Course c WITH(NOLOCK) ON cwr.CourseId = c.CourseId
	INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
	WHERE c.IsDeleted = 0
	AND v.IsDeleted = 0
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
