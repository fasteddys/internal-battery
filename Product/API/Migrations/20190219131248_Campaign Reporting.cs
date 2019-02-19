using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class CampaignReporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * 
             *  These entity's need to be added to the dbContext to support executing reporting stored procedures which return
             *  values as defined the following entities.  They do NOT need to be created as physical tables
             *  
             *  
             
            migrationBuilder.CreateTable(
                name: "CampaignDetail",
                columns: table => new
                {
                    CourseName = table.Column<string>(nullable: false),
                    Rebatetype = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    OpenEmail = table.Column<int>(nullable: false),
                    VisitLandingPage = table.Column<int>(nullable: false),
                    CreateAcount = table.Column<int>(nullable: false),
                    CourseEnrollment = table.Column<int>(nullable: false),
                    CourseCompletion = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignDetail", x => x.CourseName);
                });

            migrationBuilder.CreateTable(
                name: "CampaignStatistic",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    EmailsSent = table.Column<int>(nullable: false),
                    OpenEmail = table.Column<int>(nullable: false),
                    VisitLandingPage = table.Column<int>(nullable: false),
                    CreateAcount = table.Column<int>(nullable: false),
                    CourseEnrollment = table.Column<int>(nullable: false),
                    CourseCompletion = table.Column<int>(nullable: false),
                    CampaignGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStatistic", x => x.Name);
                });
                */

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
CREATE PROCEDURE [dbo].[System_CampaignDetails]
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
CREATE PROCEDURE [dbo].[System_CampaignStatistics] 
AS
BEGIN

 select [CampaignGuid],[name],startDate,endDate,
(select count(*) from CampaignContact cc where  cc.CampaignId = c.CampaignId) as EmailsSent,
(select count(*) from ContactAction ca where ActionId = 1 and ca.CampaignId = c.CampaignId) as OpenEmail,
(select count(*) from ContactAction ca where ActionId = 2 and ca.CampaignId = c.CampaignId) as VisitLandingPage,
(select count(*) from ContactAction ca where ActionId = 3 and ca.CampaignId = c.CampaignId) as CreateAcount,
(select count(*) from ContactAction ca where ActionId = 4 and ca.CampaignId = c.CampaignId) as CourseEnrollment,
(select count(*) from ContactAction ca where ActionId = 5 and ca.CampaignId = c.CampaignId) as CourseCompletion
 from campaign as c order by StartDate desc ;
	 
END
')
            ");

        }



    

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            /*
            migrationBuilder.DropTable(
                name: "CampaignDetail");

            migrationBuilder.DropTable(
                name: "CampaignStatistic");
            */

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_CampaignDetails]
            ");

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_CampaignStatistics]
            ");
        }
    }
}
