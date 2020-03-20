using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedSystem_Create_G2Profilestoinitbooleanstonull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('/*
<remarks>
2020.03.09 - Jab - Created
2020.03.11 - Jab - Modified to only unclude companies with active subscribers
2020.03.17 - Jab Fixed bug with undeleting deleted profiles.  Also fixed issue with finding companies with active recruiters
2020.03.18 - Jab - Modified to default IsWillingtoTravel, IsActiveJobSeeker,IsCurrentlyEmployed and IsWillingToWorkProBono to null
 
</remarks>
<description>
Adds a subcriber g2 profile record for every active subscriber for every active company
</description>
<example>
EXEC [dbo].[System_Create_G2Profiles] @SubscriberGuid = ''''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''''
</example>
*/
ALTER PROCEDURE [dbo].[System_Create_G2Profiles]  
AS
BEGIN 
 
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
	,null as IsWillingToTravel
	,null as IsActivejobSeeker
	,null as IsCurrentlyEmployed
	,null as IsWillingToWorkProBono
	,0 as CurrentRate
	,0 as DesiredRate
	,null as Goals
	,null as Preferences
	,1 as AzureIndexStatusId
FROM Subscriber  s
LEFT JOIN Company c on c.IsDeleted = 0 and c.CompanyName != ''public data'' and c.CompanyId in 
(
select c.companyId 
from recruiter r
inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
inner join Company c on rc.CompanyId = c.CompanyId
inner join Subscriber s on r.SubscriberId = s.SubscriberId
where s.IsDeleted = 0 
)
Where s.IsDeleted = 0 and  not exists ( select * from g2.Profiles where CompanyId  = c.CompanyId and SubscriberId  = s.SubscriberId )
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

return  @@ROWCOUNT
END
')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('/*
<remarks>
2020.03.09 - Jab - Created
2020.03.11 - Jab - Modified to only unclude companies with active subscribers
2020.03.17 - Jab Fixed bug with undeleting deleted profiles.  Also fixed issue with finding companies with active recruiters
 
</remarks>
<description>
Adds a subcriber g2 profile record for every active subscriber for every active company
</description>
<example>
EXEC [dbo].[System_Create_G2Profiles] @SubscriberGuid = ''''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''''
</example>
*/
ALTER PROCEDURE [dbo].[System_Create_G2Profiles]  
AS
BEGIN 
 
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
LEFT JOIN Company c on c.IsDeleted = 0 and c.CompanyName != ''public data'' and c.CompanyId in 
(
select c.companyId 
from recruiter r
inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
inner join Company c on rc.CompanyId = c.CompanyId
inner join Subscriber s on r.SubscriberId = s.SubscriberId
where s.IsDeleted = 0 
)
Where s.IsDeleted = 0 and  not exists ( select * from g2.Profiles where CompanyId  = c.CompanyId and SubscriberId  = s.SubscriberId )
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

return  @@ROWCOUNT
END
')");
        }
    }
}
