using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class coursedetailstoredprocedurefix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property
2020.01.20 - Bill Koenig - Added TotalRecords to result set
</remarks>
<description>
Retrieves course by course identifier
</description>
<example>
EXEC [dbo].[System_Get_Course] @CourseGuid = ''1BB354E8-6188-4A0B-BF39-DDAE615D9CC1''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_Course] (
    @CourseGuid UNIQUEIDENTIFIER
)
AS
BEGIN 
    SELECT TOP 1 c.Name AS Title
        ,NULL AS Duration
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
        ,c.Code
        ,cl.Name as Level
        ,c.CreateDate
        ,c.ModifyDate
        ,c.IsDeleted
        ,c.TabletImage
        ,c.DesktopImage
        ,c.MobileImage
        ,c.ThumbnailImage
        ,c.ExternalUrl
        ,t.Name as Topic
        ,(
        		SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
        		FROM CourseSkill cs
        		INNER JOIN Skill s ON s.SkillId = cs.SkillId

        		WHERE 
        			cs.CourseId = c.CourseId AND
        			cs.IsDeleted = 0 AND 
        			s.IsDeleted = 0
        ) CourseSkills
		, 1 [TotalRecords]
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.CourseGuid = @CourseGuid
END   
                ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
