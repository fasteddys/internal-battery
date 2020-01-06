using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class fixSubscriberIsVerifiedreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.09 - Jim Brazil - Created
2019.12.09 - Bill Koenig - Removed reference to IsVerified column and replaced with new logic based on 
combination of legacy column (IsEmailVerifiedLegacy) and the new Auth0 audit columns.
<description>
Returns subscribers and their source information
</description>
*/
ALTER PROCEDURE [dbo].[System_Get_New_Subscribers]  
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
    CASE WHEN LastSignIn IS NOT NULL THEN 1 WHEN IsEmailVerifiedLegacy = 1 THEN 1 ELSE 0 END [IsVerified] , 
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
            ')");

            migrationBuilder.Sql(@"
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsVerified'
          AND Object_ID = Object_ID(N'dbo.Subscriber'))
BEGIN
	ALTER TABLE dbo.Subscriber DROP COLUMN IsVerified
END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsVerified'
          AND Object_ID = Object_ID(N'dbo.Subscriber'))
BEGIN
	ALTER TABLE dbo.Subscriber ADD [IsVerified] AS ([IsEmailVerifiedLegacy]) PERSISTED NOT NULL
END
            ");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.09 - Jim Brazil - Created

<description>
Returns subscribers and there source information
TODO rewrite this SQL
</description>
    	  
*/
ALTER PROCEDURE [dbo].[System_Get_New_Subscribers]  
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
            ')");
        }
    }
}
