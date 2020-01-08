using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingv_CourseIndexerView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
            
    /*
    <remarks>
    2019-12-13 - Jim Brazil - Created
    </remarks>
    <description>
    Returns subscriber notification read counts grouped by notification title and create date
    </description>
    <example>
    SELECT * FROM [dbo].[v_NotificationReadCounts]
    </example>
    */
    CREATE VIEW [dbo].[v_CourseIndexerView] 
    AS 

  
		SELECT 
			c.Name as Title,
			c.Description,
			c.Code,
			cl.Name as Level,
			t.Name as Topic,
			c.CourseGuid,
			c.CreateDate,
			c.ModifyDate,
			c.IsDeleted,
			c.TabletImage,
			c.DesktopImage,
			c.MobileImage,
			v.Name as  VendorName,
			v.VendorGuid,
			v.LogoUrl as VendorLogoUrl,
			
			(
				SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
				FROM CourseSkill cs
				INNER JOIN Skill s ON s.SkillId = cs.SkillId

				WHERE 
					cs.CourseId = c.CourseId AND
					cs.IsDeleted = 0 AND 
					s.IsDeleted = 0
			 ) CourseSkills
		FROM  
			course c 
			INNER JOIN CourseLevel cl on c.CourseLevelId  = cl.CourseLevelId
			INNER JOIN Topic t on c.TopicId = t.TopicId
			INNER JOIN Vendor v on c.VendorId = v.VendorId
		WHERE
		   	c.IsDeleted = 0
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop view [dbo].[v_CourseIndexerView]");
        }
    }
}
