using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class createsprocSystem_Get_CoursesRandom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
   /*
    <remarks>
    2019.10.24 - Jim Brazil - Created 
	</remarks>
	<description>
	 Select random courses 
	</description>
    */
    CREATE PROCEDURE [dbo].[System_Get_CoursesRandom] (
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
	FROM  Course c  
		left join Vendor v on v.VendorId = c.VendorId
 
	ORDER by newid()          
   END
')
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_CoursesRandom]
            ");

        }
    }
}
