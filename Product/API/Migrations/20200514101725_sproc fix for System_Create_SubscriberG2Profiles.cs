using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class sprocfixforSystem_Create_SubscriberG2Profiles : Migration
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
2020.05.04 - Bill Koenig - Only create profiles in the CareerCircle company
2020.05.14 - JAB added guard to make this stored procedure indepotent 
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
    	WHERE c.IsDeleted = 0
		AND C.CompanyId IN (
			select c.companyId 
    		from recruiter r
    		inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
    		inner join Company c on rc.CompanyId = c.CompanyId
    		inner join Subscriber s on r.SubscriberId = s.SubscriberId
    		where s.IsDeleted = 0 
		)
	    AND NOT EXISTS ( select companyid from g2.Profiles p where IsDeleted = 0 and  p.SubscriberId = s.SubscriberId and  p.CompanyId = c.CompanyId )
 
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
              migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.28 - Jab - Created
2020.03.02 - JAB - Modified to undelete user profile records before adding new profile records.  Also modified to return rows affected from both the update and insert statements
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
2020.03.29 - Bill Koenig - Forcibly truncate data that exceeds column length limits in profile. The column length limits for tables in the G2 profile were chosen carefully and
    it is safe to assume that data that exceeds these limits is bogus. Column length limits are important for future performance tuning needs (e.g. limitations on conventional 
    indexing for large text fields).
2020.05.04 - Bill Koenig - Only create profiles in the CareerCircle company
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
    	WHERE c.IsDeleted = 0
		AND C.CompanyId IN (
			select c.companyId 
    		from recruiter r
    		inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
    		inner join Company c on rc.CompanyId = c.CompanyId
    		inner join Subscriber s on r.SubscriberId = s.SubscriberId
    		where s.IsDeleted = 0 
		)
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
    }
}
