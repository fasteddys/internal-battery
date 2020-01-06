using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_CourseVariantssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
            
    
/*
<remarks>
2019.12.17 - Jim Brazil - Created
</remarks>
<description>
Retrieves coursevariants by course identifier
</description>
*/
CREATE PROCEDURE [dbo].[System_Get_CourseVariants] (
    @CourseGuid UNIQUEIDENTIFIER
)
AS
BEGIN 
    SELECT c.Name AS Title
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
        ,c.TabletImage as ThumbnailUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid,
		cv.CourseVariantGuid,
		cv.Price,
		cvt.Name as CourseVariantType,
		c.Code,
		cl.Name as Level,
		c.CreateDate,
		c.ModifyDate,
		c.IsDeleted,
		c.TabletImage,
		c.DesktopImage,
		c.MobileImage,
		t.Name as Topic,

		
			(
				SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
				FROM CourseSkill cs
				INNER JOIN Skill s ON s.SkillId = cs.SkillId

				WHERE 
					cs.CourseId = c.CourseId AND
					cs.IsDeleted = 0 AND 
					s.IsDeleted = 0
			 ) CourseSkills
    FROM Course c
	LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
	LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
	LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.CourseGuid = @CourseGuid
END
            ')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"drop procedure [dbo].[System_Get_CourseVariants]");
        }
    }
}
