using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Course_Random_IsDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019.10.24 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level, cleaned up formatting
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property
2020.01.21 - Bill Koenig - Added TotalRecords to SELECT output
2020.01.22 - Jyoti Guin - Added IsDeleted = 0 to filter out deleted courses
</remarks>
<description>
Select random courses 
</description>
<example>
EXEC [dbo].[System_Get_CoursesRandom] @MaxResults = 5
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesRandom] (
    @MaxResults INT
)
AS
BEGIN 
    SELECT TOP (@MaxResults)
    c.Name as Title ,
    	null as Duration,
    	c.Description, 
    	(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
    	v.Name as VendorName,
    	c.CourseGuid,
    	v.LogoUrl as VendorLogoUrl ,
    	v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid
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
		, 0 [TotalRecords]
    FROM  Course c  
        LEFT JOIN Vendor v ON v.VendorId = c.VendorId
        LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    	LEFT JOIN Topic t on c.TopicId = t.TopicId
        WHERE c.IsDeleted = 0
    ORDER by newid()          
END
                ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
