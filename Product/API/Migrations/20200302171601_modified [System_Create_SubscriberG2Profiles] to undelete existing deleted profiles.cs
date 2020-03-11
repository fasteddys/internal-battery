using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifiedSystem_Create_SubscriberG2Profilestoundeleteexistingdeletedprofiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.28 - Jab - Created
2020.03.02 - JAB - Modified to undelete user profile records before adding new profile records.  Also modified to return rows affected from both the update and insert statements

</remarks>
<description>
Adds a subcriber g2 profile record for a new subscriber.  1 record per active company is created 
</description>
<example>
EXEC [dbo].[[System_Create_SubscriberG2Profiles]] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
ALTER PROCEDURE [dbo].[System_Create_SubscriberG2Profiles] (
	@SubscriberGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 


    DECLARE @RowsAffected INT;

   --- First restore any previously deleted profiles for the user 
    Update G2.Profiles SET IsDeleted = 0  
	WHERE IsDeleted = 1 and SubscriberId = (  SELECT SubscriberId FROM Subscriber WHERE SubscriberGuid = @SubscriberGuid )

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
FROM Company c
LEFT JOIN subscriber s on s.SubscriberGuid = @SubscriberGuid
WHERE C.CompanyId NOT IN ( Select CompanyId from g2.Profiles where SubscriberGuid = @SubscriberGuid) and c.IsDeleted = 0
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
)
Select * from ProfilesToInsert

Set @RowsAffected = @RowsAffected + @@ROWCOUNT	 

return @RowsAffected
END')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('   
 /*
<remarks>
2020.02.28 - Jab - Created

</remarks>
<description>
Adds a subcriber g2 profile record for a new subscriber.  1 record per active company is created 
</description>
<example>
EXEC [dbo].[[System_Create_SubscriberG2Profiles]] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
ALTER PROCEDURE [dbo].[System_Create_SubscriberG2Profiles] (
	@SubscriberGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 
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
FROM Company c
LEFT JOIN subscriber s on s.SubscriberGuid = @SubscriberGuid
WHERE C.CompanyId NOT IN ( Select CompanyId from g2.Profiles where SubscriberGuid = @SubscriberGuid )
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
)
Select * from ProfilesToInsert
	 
return @@RowCount
END
            ')");





        }
    }
}
