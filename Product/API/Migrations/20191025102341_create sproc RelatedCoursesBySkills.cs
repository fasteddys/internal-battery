using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class createsprocRelatedCoursesBySkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*
 <remarks>
2019.10.23 - Bill Koenig - Created
</remarks>
<description>
Retrieves a list of courses that are associated with the skills provided in the histogram. Results are
</description>
<example>
DECLARE @SkillHistogram AS [dbo].[SkillHistogram]
INSERT INTO @SkillHistogram
SELECT Skill, Occurrences
FROM (VALUES 
    (''authentication'', 10), 
    (''ruby'', 3), 
    (''python'', 2), 
    (''asp.net'', 1), 
    (''c#.net'', 1), 
    (''css'', 1), 
    (''html'', 1), 
    (''javascript'', 1), 
    (''json'', 1), 
    (''nodejs'', 1), 
    (''npm'', 1), 
    (''sql'', 1), 
    (''ssis'', 1), 
    (''ssrs'', 1), 
    (''xml'', 1)) AS SkillHistogram(Skill, Occurrences)
EXEC [dbo].[System_Get_RelatedCoursesBySkills] @SkillHistogram = @SkillHistogram, @MaxResults = 5
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_RelatedCoursesBySkills] (
    @SkillHistogram dbo.SkillHistogram READONLY
    , @MaxResults INT = NULL 
)
AS
BEGIN
    SELECT TOP (ISNULL(@MaxResults, 2147483647)) 
        c.CourseGuid, 
		c.Description,
        c.[Name] [Title], 
        0 [LessonCount], 
        0 [CourseLevel],         
        v.VendorGuid, 
        NULL [VendorLogoUrl], 
        SUM(sh.Occurrences) [Rank], 
		null as Duration,
		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments,				
		0  as NewFlag,
		v.Name as VendorName,				 
		0 as NumLessons 
    FROM Course c WITH(NOLOCK)
    INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
    INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
    INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
    INNER JOIN @SkillHistogram sh ON s.SkillName = sh.Skill
    GROUP BY c.CourseGuid, c.CourseId,v.Name, c.[Name], v.VendorGuid, c.Description
    ORDER BY [Rank] DESC 
END
')
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_RelatedCoursesBySkills]
            ");
        }
    }
}
