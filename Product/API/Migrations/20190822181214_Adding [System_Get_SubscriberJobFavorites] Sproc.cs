using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Get_SubscriberJobFavoritesSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
EXEC('
 
    /*
    <remarks>
    2019.08.22 - Jim Brazil - Created

	<description>
    Returns job favorite information for the specified subscriber
    </description>
	  
    */
    CREATE PROCEDURE [dbo].[System_Get_SubscriberJobFavorites] (
        @SubscriberId Int 
    )
    AS
    BEGIN

	SELECT  
      jpf.JobPostingFavoriteGuid, 
	  jp.JobPostingGuid, 
	  jp.Title, 
	  jp.city + '', '' +  jp.Province as CityProvince, 
	  jp.PostingExpirationDateUTC, 
	  c.CompanyName, 
	  ja.JobApplicationId, 
	  jpf.SubscriberId
	 FROM 
     JobPosting jp
     left join JobApplication ja on ja.JobPostingId = jp.JobPostingId , 
	 Company c,
	 JobPostingFavorite jpf
	 WHERE jp.JobPostingId = jpf.JobPostingId and jp.IsDeleted = 0 and jp.CompanyId = c.CompanyId  and jpf.IsDeleted = 0 and jpf.SubscriberId = @SubscriberId
   
    END

')
            ");



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_SubscriberJobFavorites]
            ");
        }
    }
}
