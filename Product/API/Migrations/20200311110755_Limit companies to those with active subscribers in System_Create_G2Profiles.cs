using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class LimitcompaniestothosewithactivesubscribersinSystem_Create_G2Profiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC(' 
 /*
<remarks>
2020.03.09 - Jab - Created
2020.03.11 - Jab - Modified to only unclude companies with active subscribers
 
</remarks>
<description>
Adds a subcriber g2 profile record for every active subscriber for every active company
</description>
<example>
EXEC [dbo].[System_Create_G2Profiles] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [dbo].[System_Create_G2Profiles]  
AS
BEGIN 


    DECLARE @RowsAffected INT;

   --- First restore any previously deleted profiles for active subscribers and active companies 
    Update G2.Profiles SET IsDeleted = 0  
		WHERE IsDeleted = 1 and CompanyId  in  (  SELECT CompanyId FROM Company WHERE IsDeleted = 0 ) and SubscriberId in  ( Select SubscriberId FROM Subscriber where IsDeleted = 0 )

	Set @RowsAffected = @@RowCount;
   -- Add new G2s 

	;With ProfilesToInsert AS
(
	SELECT 
	0 as IsDeleted
	,getutcdate() as CreateDate
	,null as Modifydate
	,''00000000-0000-0000-0000-000000000000'' as CreateGuid
	,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
	,newid() as ProfileGuid
	,c.Companyid
	,s.subscriberid
	,s.FirstName
	,s.Email
	,s.PhoneNumber
	,null as ContactTypeId
	,s.Address as StreetAddress
	,null as CityId
	,s.StateId
	,null as PostalId
	,null as ExperienceLevelId
	,null as EmploymentTypeId
	,s.Title
	,0 as IsWillingToTravel
	,0 as IsActivejobSeeker
	,0 as IsCurrentlyEmployed
	,0 as IsWillingToWorkProBono
	,0 as CurrentRate
	,0 as DesiredRate
	,null as Goals
	,null as Preferences
	,1 as AzureIndexStatusId
FROM Subscriber  s
LEFT JOIN Company c on c.IsDeleted = 0 and c.CompanyName != ''public data'' and c.CompanyId in ( select distinct companyId from Recruiter where subscriberid is not null )
Where s.IsDeleted = 0 and  not exists ( select * from g2.Profiles where CompanyId  = c.CompanyId and SubscriberId  = s.SubscriberId and IsDeleted = 0 )
)
INSERT INTO g2.Profiles   
(
    IsDeleted
   ,CreateDate
   ,Modifydate
   ,CreateGuid
   ,ModifyGuid
   ,ProfileGuid
   ,Companyid
   ,Subscriberid
   ,FirstName
   ,Email
   ,PhoneNumber
   ,ContactTypeId
   ,StreetAddress
   ,CityId
   ,StateId
   ,PostalId
   ,ExperienceLevelId
   ,EmploymentTypeId
   ,Title
   ,IsWillingToTravel
   ,IsActivejobSeeker
   ,IsCurrentlyEmployed
   ,IsWillingToWorkProBono
   ,CurrentRate
   ,DesiredRate
   ,Goals
   ,Preferences
   ,AzureIndexStatusId
)
Select * from ProfilesToInsert

Set @RowsAffected = @RowsAffected + @@ROWCOUNT	 

return @RowsAffected
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Create_G2Profiles]
            ");
        }
    }
}
