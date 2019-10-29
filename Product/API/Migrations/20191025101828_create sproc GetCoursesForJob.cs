using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class createsprocGetCoursesForJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 
            migrationBuilder.Sql(@"
EXEC('
/*
    <remarks>
    2019.10.23 - Jim Brazil - Created 
    */
    CREATE PROCEDURE [dbo].[System_Get_CoursesForJob] (
        @JobGuid UniqueIdentifier,
		@MaxResults INT
    )
    AS
    BEGIN 
		Select  top(@MaxResults)
		c.Name as Title ,
			null as Duration,
			c.Description, 
			(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
			0  as NewFlag,
			v.Name as VendorName,
			c.CourseGuid,
			0 as CourseLevel,	
			0 as NumLessons,
			null as VendorLogoUrl,
			v.VendorGuid
	FROM CourseSkill cs
		left join Skill s on s.SkillId = cs.SkillId
		left join Course c on cs.CourseId = c.CourseId
		left join Vendor v on v.VendorId = c.VendorId
	WHERE s.SkillName in 
		(SELECT 
			s.SkillName 
		FROM 
			jobpostingskill jps
			left join skill s on jps.SkillId = s.SkillId
		WHERE 
			JobPostingId = (select JobPostingId from JobPosting where JobPostingGuid =  @JobGuid )
		) AND
			c.IsDeleted = 0
	ORDER by NumEnrollments DESC           
   END
')
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_CoursesForJob]
            ");

        }
    }
}
