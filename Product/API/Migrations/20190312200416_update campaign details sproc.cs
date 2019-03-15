using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatecampaigndetailssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.03.12 - Bill Koenig - Modified logic to return campaigns which do not have course offers, exclude logical deletes, ordered output, added example to comment block
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
EXEC [dbo].[System_CampaignDetails] @CampaignGuid = ''99C72F03-FF31-4A82-937B-16926EE0D9A2''
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
    @CampaignGuid UNIQUEIDENTIFIER 
AS
BEGIN

    SELECT 
		c.Name CourseName, 
		rt.Name RebateType, 
		co.Email, 
		co.FirstName, 
		co.LastName, 
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 1 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as OpenEmail,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 2 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as VisitLandingPage,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 3 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CreateAcount,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 4 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CourseEnrollment,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 5 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CourseCompletion
    FROM Campaign cp
		INNER JOIN CampaignContact cc ON cp.CampaignId = cc.CampaignId
		INNER JOIN Contact co ON cc.ContactId = co.ContactId
		LEFT JOIN CampaignCourseVariant ccv ON ccv.CampaignId = cp.CampaignId
		LEFT JOIN CourseVariant cv ON ccv.CourseVariantId = cv.CourseVariantId
		LEFT JOIN Course c ON cv.CourseId = c.CourseId
		LEFT JOIN RebateType rt ON ccv.RebateTypeId = rt.RebateTypeId
    WHERE cp.CampaignGuid = @CampaignGuid 
		AND co.IsDeleted = 0
		AND cp.IsDeleted = 0
		AND cc.IsDeleted = 0
	ORDER BY OpenEmail DESC, VisitLandingPage DESC, CreateAcount DESC, CourseEnrollment DESC, CourseCompletion DESC

END
')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
EXEC('
 

/*
<remarks>
2019.02.13=8 - Jim Brazil - Created 
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
 @CampaignGuid uniqueidentifier 
AS
BEGIN

 select 
	Course.Name as CourseName, 
	RebateType.Name as RebateType, 
	Contact.Email, 
	Contact.FirstName, 
	Contact.LastName, 
	(select count(*) from ContactAction ca where ActionId = 1 and ca.CampaignId = Campaign.CampaignId and ca.ContactId = Contact.ContactId) as OpenEmail,
	(select count(*) from ContactAction ca where ActionId = 2 and ca.CampaignId = Campaign.CampaignId and ca.ContactId = Contact.ContactId) as VisitLandingPage,
    (select count(*) from ContactAction ca where ActionId = 3 and ca.CampaignId = Campaign.CampaignId and ca.ContactId = Contact.ContactId) as CreateAcount,
	(select count(*) from ContactAction ca where ActionId = 4 and ca.CampaignId = Campaign.CampaignId and ca.ContactId = Contact.ContactId) as CourseEnrollment,
	(select count(*) from ContactAction ca where ActionId = 5 and ca.CampaignId = Campaign.CampaignId and ca.ContactId = Contact.ContactId) as CourseCompletion
  
  from 
	Campaign Campaign, 
	CampaignContact CampaignContact,	
	Contact Contact, 
	CampaignCourseVariant CampaignCourseVariant, 
	CourseVariant CourseVariant, 
	Course Course, 
	RebateType RebateType
  
  where 
	Campaign.CampaignGuid =  @CampaignGuid and 
	CampaignContact.CampaignId = Campaign.CampaignId and 
	CampaignContact.ContactId = Contact.ContactId and 
	CampaignCourseVariant.CampaignId = Campaign.CampaignId and 
	CampaignCourseVariant.CourseVariantId = CourseVariant.CourseVariantId and 
	CourseVariant.CourseId = course.CourseId and 
	RebateType.RebateTypeId = CampaignCourseVariant.RebateTypeId
	 
END
')
            ");
        }
    }
}
