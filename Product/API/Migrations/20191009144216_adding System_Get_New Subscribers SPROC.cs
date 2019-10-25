using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_NewSubscribersSPROC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
EXEC('
 
    /*
    <remarks>
    2019.10.09 - Jim Brazil - Created

	<description>
    Returns subscribers and there source information
	TODO rewrite this SQL
    </description>
	  
    */
    CREATE PROCEDURE [dbo].[System_Get_New_Subscribers ]  
    AS
    BEGIN

	  SELECT 
		v.SubscriberId, 
		s.createdate, 
		s.Email, 
		s.FirstName, 
		s.LastName, 	
		s.PhoneNumber, 
		s.City, 
		st.[Name] [State], 
		s.PostalCode, 
		s.IsVerified , 
		v.PartnerName, 
		v.GroupName,
	    ( SELECT 
			json_value(ProfileData, ''$.referer'') OldSource  
		  FROM  SubscriberProfileStagingStore 
		  WHERE 
		    isdeleted = 0 and 
	       profileformat =''json'' and 
		   ProfileSource = ''CareerCircle'' and 
		   Subscriberid = s.subscriberid) LegacySource,
	       (
		     SELECT count(*) from Enrollment_vw 
			 WHERE 
			     Enrollment_vw.Subscriberid = s.SubscriberId and 
				 Enrollment_vw.IsDeleted=0
	       ) WozEnrollmentCount
		  FROM Subscriber s
		  LEFT JOIN v_SubscriberSourceDetails v on s.SubscriberId = v.SubscriberId
		  LEFT JOIN [State] st on  s.stateid =  st.stateid
		  WHERE 
		     s.IsDeleted=0 and 
			 ( v.GroupRank is null or v.GroupRank = 1) 
	ORDER BY
		s.CreateDate DESC 
   
    END

')
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_New Subscribers]
            ");
        }

    }
}
