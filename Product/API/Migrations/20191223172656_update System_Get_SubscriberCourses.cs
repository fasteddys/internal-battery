using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updateSystem_Get_SubscriberCourses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"Exec('           

   
/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
2016.12.23 - Jim Brazil - Updated to NVL grade and also return enrollment status
 
</remarks>
<description>
Returns courses for subscriber 
</description>
 
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberCourses] (
		    @SubscriberGuid UNIQUEIDENTIFIER,
			@ExcludeCompleted int,
			@ExcludeActive int

        )
        AS
BEGIN 

SELECT 
	ISNULL(e.grade,0) as Grade, 
	e.PercentComplete,
	c.Name as Title,
	c.Description,
	cl.Name as CourseLevel,
	0 as NumLessons,
	v.Name as VendorName,
	v.VendorGuid,
	v.LogoUrl as VendorLogoUrl,
	e.EnrollmentStatusId

FROM 
Enrollment e
join Subscriber s on e.SubscriberId = s.SubscriberId
Join Course c on e.CourseId = c.CourseId
Join Vendor v on c.VendorId = v.vendorId
join CourseLevel cl on cl.CourseLevelId = c.CourseLevelId
WHERE
e.IsDeleted = 0 and  
(   
   ( @ExcludeCompleted = 0 and @ExcludeActive = 0 and 1 = 1  ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 0 and e.PercentComplete < 100 ) OR
   ( @ExcludeCompleted = 0 and @ExcludeActive = 1 and e.PercentComplete = 100 ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 1 and 1 = 0 ) 
)

and s.SubscriberGuid = @SubscriberGuid
Order by e.ModifyDate desc       
END

            ')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('           
     
/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
 
 
</remarks>
<description>
Returns courses for subscriber 
</description>
 
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberCourses] (
		    @SubscriberGuid UNIQUEIDENTIFIER,
			@ExcludeCompleted int,
			@ExcludeActive int

        )
        AS
BEGIN 

SELECT 
	e.grade,
	e.PercentComplete,
	c.Name as Title,
	c.Description,
	cl.Name as CourseLevel,
	0 as NumLessons,
	v.Name as VendorName,
	v.VendorGuid,
	v.LogoUrl as VendorLogoUrl 

FROM 
Enrollment e
join Subscriber s on e.SubscriberId = s.SubscriberId
Join Course c on e.CourseId = c.CourseId
Join Vendor v on c.VendorId = v.vendorId
join CourseLevel cl on cl.CourseLevelId = c.CourseLevelId
WHERE
e.IsDeleted = 0 and  
(   
   ( @ExcludeCompleted = 0 and @ExcludeActive = 0 and 1 = 1  ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 0 and e.PercentComplete < 100 ) OR
   ( @ExcludeCompleted = 0 and @ExcludeActive = 1 and e.PercentComplete = 100 ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 1 and 1 = 0 ) 
)

and s.SubscriberGuid = @SubscriberGuid
Order by e.ModifyDate desc       
END

            ')");


        }
    }
}
