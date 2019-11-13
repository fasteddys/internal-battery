using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class fixkeywordandlocationintellisense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.23 - Bill Koenig - Created
</remarks>
<description>
Retrieves unique values for the autocomplete feature of the keyword input on the job search page
</description>
<example>
EXEC [dbo].[System_Get_KeywordSearchTerms]
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_KeywordSearchTerms]
AS
BEGIN

	;WITH jobSearchKeywordData AS (
		SELECT j.Title, c.CompanyName, CAST(s.SkillName as NVARCHAR(MAX)) [SkillName]
		FROM dbo.JobPosting j WITH(NOLOCK)
		INNER JOIN dbo.Company c WITH(NOLOCK) ON j.CompanyId = c.CompanyId
		INNER JOIN dbo.JobPostingSkill jps WITH(NOLOCK) ON jps.JobPostingId = j.JobPostingId
		INNER JOIN dbo.Skill s WITH(NOLOCK) ON s.SkillId = jps.SkillId
		WHERE j.IsDeleted = 0
		AND c.IsDeleted = 0
		AND jps.IsDeleted = 0
		AND s.IsDeleted = 0
	)
	SELECT DISTINCT unpvt.[Type], LOWER(unpvt.[Value]) [Value]
	FROM jobSearchKeywordData
	UNPIVOT ([Value] FOR [Type] IN (Title, CompanyName, SkillName)) as unpvt

END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.23 - Bill Koenig - Created
</remarks>
<description>
Retrieves unique values for the autocomplete feature of the location input on the job search page
</description>
<example>
EXEC [dbo].[System_Get_LocationSearchTerms]
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_LocationSearchTerms]
AS
BEGIN

	;WITH jobSearchKeywordData AS (
		SELECT j.City, j.Province [State], j.PostalCode
		FROM dbo.JobPosting j WITH(NOLOCK)
		WHERE j.IsDeleted = 0
	)
	SELECT DISTINCT unpvt.[Type], LOWER(unpvt.[Value]) [Value]
	FROM jobSearchKeywordData
	UNPIVOT ([Value] FOR [Type] IN (City, [State], PostalCode)) as unpvt

END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_KeywordSearchTerms]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_LocationSearchTerms]");
        }
    }
}
