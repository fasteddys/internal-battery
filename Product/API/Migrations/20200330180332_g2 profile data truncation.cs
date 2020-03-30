using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class g2profiledatatruncation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.28 - Jab - Created
2020.03.02 - JAB - Modified to undelete user profile records before adding new profile records.  Also modified to return rows affected from both the update and insert statements
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
2020.03.29 - Bill Koenig - Forcibly truncate data that exceeds column length limits in profile. The column length limits for tables in the G2 profile were chosen carefully and
	it is safe to assume that data that exceeds these limits is bogus. Column length limits are important for future performance tuning needs (e.g. limitations on conventional 
	indexing for large text fields).
</remarks>
<description>
Adds a subcriber g2 profile record for a new subscriber.  1 record per active company is created 
</description>
<example>
EXEC [G2].[System_Create_SubscriberG2Profiles] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
ALTER PROCEDURE [G2].[System_Create_SubscriberG2Profiles] (
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

	;With ProfilesToInsert AS (
		SELECT 
		0 as IsDeleted
		,getutcdate() as CreateDate
		,null as Modifydate
		,''00000000-0000-0000-0000-000000000000'' as CreateGuid
		,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
		,newid() as ProfileGuid
		,c.Companyid
		,s.subscriberid
		,SUBSTRING(s.FirstName, 1, 100) FirstName
		,SUBSTRING(s.Email, 1, 254) Email
		,SUBSTRING(s.PhoneNumber, 1, 20) PhoneNumber
		,null as ContactTypeId
		,SUBSTRING(s.[Address], 1, 100) as StreetAddress
		,null as CityId
		,s.StateId
		,null as PostalId
		,null as ExperienceLevelId
		,SUBSTRING(s.Title, 1, 100) Title
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

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.09 - Jab - Created
2020.03.11 - Jab - Modified to only unclude companies with active subscribers
2020.03.17 - Jab Fixed bug with undeleting deleted profiles.  Also fixed issue with finding companies with active recruiters
2020.03.18 - Jab - Modified to default IsWillingtoTravel, IsActiveJobSeeker,IsCurrentlyEmployed and IsWillingToWorkProBono to null
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
2020.03.29 - Bill Koenig - Forcibly truncate data that exceeds column length limits in profile. The column length limits for tables in the G2 profile were chosen carefully and
	it is safe to assume that data that exceeds these limits is bogus. Column length limits are important for future performance tuning needs (e.g. limitations on conventional 
	indexing for large text fields). Also corrected example.
</remarks>
<description>
Adds a subcriber g2 profile record for every active subscriber for every active company
</description>
<example>
EXEC [G2].[System_Create_G2Profiles]
</example>
*/
ALTER PROCEDURE [G2].[System_Create_G2Profiles]  
AS
BEGIN  
   -- Add new G2s 
	;With ProfilesToInsert AS (
		SELECT 
		0 as IsDeleted
		,getutcdate() as CreateDate
		,null as Modifydate
		,''00000000-0000-0000-0000-000000000000'' as CreateGuid
		,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
		,newid() as ProfileGuid
		,c.Companyid
		,s.subscriberid
		,SUBSTRING(s.FirstName, 1, 100) FirstName
		,SUBSTRING(s.Email, 1, 254) Email
		,SUBSTRING(s.PhoneNumber, 1, 20) PhoneNumber
		,null as ContactTypeId
		,SUBSTRING(s.[Address], 1, 100) as StreetAddress
		,null as CityId
		,s.StateId
		,null as PostalId
		,null as ExperienceLevelId
		,SUBSTRING(s.Title, 1, 100) Title
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
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.03 - Jab - Created
2020.03.09 - Jab - Fixed bug with ProfilesToInsert CTE.  The where condition for determining which subscribers already had a profile for the specified company was wrong
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
2020.03.29 - Bill Koenig - Forcibly truncate data that exceeds column length limits in profile. The column length limits for tables in the G2 profile were chosen carefully and
	it is safe to assume that data that exceeds these limits is bogus. Column length limits are important for future performance tuning needs (e.g. limitations on conventional 
	indexing for large text fields). Also corrected example.
</remarks>
<description>
Adds a subcriber g2 profile record for a new company.  1 record per active subscriber is created 
</description>
<example>
EXEC [G2].[System_Create_CompanyG2Profiles] @CompanyGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
ALTER PROCEDURE [G2].[System_Create_CompanyG2Profiles] (
	@CompanyGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 
    DECLARE @RowsAffected INT;

   --- First restore any previously deleted profiles for the user 
    Update G2.Profiles SET IsDeleted = 0  
	WHERE IsDeleted = 1 and CompanyId = (  SELECT CompanyId FROM Company WHERE CompanyGuid = @CompanyGuid )

	Set @RowsAffected = @@RowCount;

   -- Add new G2s 

	;With ProfilesToInsert AS (
		SELECT 
		0 as IsDeleted
		,getutcdate() as CreateDate
		,null as Modifydate
		,''00000000-0000-0000-0000-000000000000'' as CreateGuid
		,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
		,newid() as ProfileGuid
		,c.Companyid
		,s.subscriberid
		,SUBSTRING(s.FirstName, 1, 100) FirstName
		,SUBSTRING(s.Email, 1, 254) Email
		,SUBSTRING(s.PhoneNumber, 1, 20) PhoneNumber
		,null as ContactTypeId
		,SUBSTRING(s.[Address], 1, 100) as StreetAddress
		,null as CityId
		,s.StateId
		,null as PostalId
		,null as ExperienceLevelId
		,SUBSTRING(s.Title, 1, 100) Title
		,0 as IsWillingToTravel
		,0 as IsActivejobSeeker
		,0 as IsCurrentlyEmployed
		,0 as IsWillingToWorkProBono
		,0 as CurrentRate
		,0 as DesiredRate
		,null as Goals
		,null as Preferences
		FROM Subscriber  s
		LEFT JOIN Company c on c.CompanyGuid = @CompanyGuid
		WHERE s.SubscriberId NOT IN ( Select SubscriberId from g2.Profiles p where p.CompanyId = (select companyid from company where CompanyGuid = @CompanyGuid) ) and s.IsDeleted = 0
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

        }
    }
}
