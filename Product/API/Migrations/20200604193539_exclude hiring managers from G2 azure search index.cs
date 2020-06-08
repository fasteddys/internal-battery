using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class excludehiringmanagersfromG2azuresearchindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-21 - Bill Koenig - Created
2020-02-25 - Bill Koenig - Changed data type for current and desired rate, removed support for one to many search locations, added single location	column which returns ''geography'''' data type, prioritized the selection of the geo data based on specificity (function), incorporated public and private profile into CTEs, included modify date that indicates the most recent change across all profiles for indexing operations, added code comments for readability
2020-02-26 - Bill Koenig - Primary CTE is now subscriberProfiles with LEFT JOINs to publicProfiles and companyProfiles so that every subscriber has a record whether or not they have a public profile (we still need to create companyProfiles for everyone though), added SubscriberGuid and CompanyGuid to the view output, added check for logical deletes on profiles
2020-03-10 - JAB - Added AzureIndexStatus 
2020-03-20 - Bill Koenig - Added support for multiple employment types for each profile
2020-03-20 - JAB - Added support for subscribe profile avatar
2020-03-20 - JAB - Added subscriber attribution source 
2020-03-20 - JAB - Adding subcriber resume blob 
2020-04-01 - JAB  - Adding support for subscriber CreateDate
2020-04-10 - JAB - Adding support for IsWillingToRelocate
2020-05-04 - Bill Koenig - Fix for STRING_AGG error on SubscriberSkill (8000 bytes)
2020-06-04 - Bill Koenig - Exclude hiring managers from G2 search index
</remarks>
<description>
Returns G2 profile data from multiple sources in a flattened format for indexing in Azure Search.
</description>
<example>
SELECT * FROM [G2].[v_ProfileAzureSearch]
</example>
*/
ALTER VIEW [G2].[v_ProfileAzureSearch]
AS
    WITH subscriberWorkHistoryWithRownum AS (
        -- returns all subscriber work history with compensation data if it is hourly
        SELECT swh.SubscriberId, swh.StartDate, swh.Title, swh.Compensation, ct.CompensationTypeName, ISNULL(swh.ModifyDate, swh.CreateDate) ModifyDate, ROW_NUMBER() OVER(PARTITION BY swh.SubscriberId ORDER BY swh.StartDate DESC) rownum
        FROM SubscriberWorkHistory swh
        LEFT JOIN CompensationType ct ON swh.CompensationTypeId = ct.CompensationTypeId
        WHERE swh.IsDeleted = 0
    ), mostRecentSubscriberWorkHistory AS (
        -- uses the previous CTE to find the most recent work history record to use for title and compensation in subscriber curated data
        SELECT SubscriberId, Title, Compensation, CompensationTypeName, ModifyDate
        FROM subscriberWorkHistoryWithRownum
        WHERE rownum = 1
    ), subscriberSkills AS (
        -- returns subscriber curated skill data
        SELECT ss.SubscriberId, CAST(s.SkillName AS NVARCHAR(MAX)) SkillName, ISNULL(ss.ModifyDate, ss.CreateDate) ModifyDate
        FROM SubscriberSkill ss
        INNER JOIN Skill s ON ss.SkillId = s.SkillId
        WHERE ss.IsDeleted = 0
        AND s.IsDeleted = 0
    ), subscriberProfiles AS (
        -- returns a flattened view of all subscriber curated data
        SELECT s.CreateDate, s.SubscriberId, s.SubscriberGuid, CASE WHEN LEN(s.FirstName) > 0 THEN s.FirstName ELSE NULL END FirstName, CASE WHEN LEN(s.LastName) > 0 THEN s.LastName ELSE NULL END LastName, s.Email, CASE WHEN LEN(PhoneNumber) > 0 THEN PhoneNumber ELSE NULL END PhoneNumber, CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress, CASE WHEN LEN(City) > 0 THEN City ELSE NULL END City, t.Code [State], CASE WHEN LEN(PostalCode) > 0 THEN PostalCode ELSE NULL END Postal, mrswh.Title, CASE WHEN mrswh.CompensationTypeName = ''Hourly'' THEN mrswh.Compensation ELSE NULL END CurrentRate, STRING_AGG(ss.SkillName, '';'') SubscriberSkills
        , dbo.fn_GetGeographyCoordinate((SELECT TOP 1 CityId FROM dbo.City c INNER JOIN dbo.[State] t ON c.StateId = t.StateId WHERE c.[Name] = s.City AND t.StateId = s.StateId), s.StateId, (SELECT TOP 1 PostalId FROM dbo.Postal WHERE Code = s.PostalCode)) [Location]
        , dbo.fn_GreatestDate(ISNULL(s.ModifyDate, s.CreateDate), mrswh.ModifyDate, MAX(ss.ModifyDate), default, default) ModifyDate, s.AvatarUrl, ssd.PartnerGuid as PartnerGuid
        FROM Subscriber s	
        LEFT JOIN [State] t ON s.StateId = t.StateId
        LEFT JOIN mostRecentSubscriberWorkHistory mrswh ON s.SubscriberId = mrswh.SubscriberId
        LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId
        LEFT JOIN v_SubscriberSourceDetails ssd on ssd.SubscriberId = s.SubscriberId and ssd.GroupRank = 1 and ssd.PartnerRank = 1
        LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
        WHERE s.IsDeleted = 0
		AND hm.HiringManagerId IS NULL
        GROUP  BY s.SubscriberId, s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.[Address], s.City, t.Code, s.PostalCode, mrswh.Title, mrswh.CompensationTypeName, mrswh.Compensation, s.StateId, s.CreateDate, s.ModifyDate, mrswh.ModifyDate, s.AvatarUrl, ssd.PartnerGuid
    ), profileTags AS (
        -- flattens tags that have been applied to a profile
        SELECT p.ProfileId, STRING_AGG(t.[Name], '';'') Tags, dbo.fn_GreatestDate(MAX(pt.CreateDate), MAX(pt.ModifyDate), default, default, default) ModifyDate
        FROM G2.Profiles p
        LEFT JOIN G2.ProfileTags pt ON p.ProfileId = pt.ProfileId
        LEFT JOIN dbo.Tag t ON pt.TagId = t.TagId
        WHERE pt.IsDeleted = 0
        GROUP BY p.ProfileId
    ), profileSkills AS (
        -- flattens skills that have been applied to a profile
        SELECT p.ProfileId, STRING_AGG(s.SkillName, '';'') Skills, dbo.fn_GreatestDate(MAX(ps.CreateDate), MAX(ps.ModifyDate), default, default, default) ModifyDate
        FROM G2.Profiles p
        LEFT JOIN G2.ProfileSkills ps ON p.ProfileId = ps.ProfileId
        LEFT JOIN dbo.Skill s ON s.SkillId = ps.SkillId
        WHERE ps.IsDeleted = 0
        GROUP BY p.ProfileId
    ), profileEmploymentTypes AS (
        -- flattens employment types that have been applied to a profile
        SELECT p.ProfileId, STRING_AGG(et.[Name], '';'') EmploymentTypes, dbo.fn_GreatestDate(MAX(pet.CreateDate), MAX(pet.ModifyDate), default, default, default) ModifyDate
        FROM G2.Profiles p
        LEFT JOIN G2.ProfileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
        LEFT JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
        WHERE pet.IsDeleted = 0
        GROUP BY p.ProfileId		
    ), publicProfiles AS (
        -- public profiles contains the ''base'' profile image as well as data extracted from third party sources such as Connected
        SELECT  p.IsWillingToRelocate, p.ProfileGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, ps.Skills PublicSkills, NULL PrivateSkills, pet.EmploymentTypes
        , dbo.fn_GetGeographyCoordinate(p.CityId, p.StateId, p.PostalId) [Location]
        , dbo.fn_GreatestDate(ISNULL(p.ModifyDate, p.CreateDate), pt.ModifyDate, ps.ModifyDate, default, default) ModifyDate
        FROM G2.Profiles p
        INNER JOIN Subscriber su ON p.SubscriberId = su.SubscriberId
        INNER JOIN dbo.Company co ON p.CompanyId = co.CompanyId
        LEFT JOIN G2.ContactTypes ct ON p.ContactTypeId = ct.ContactTypeId
        LEFT JOIN City c ON p.CityId = c.CityId
        LEFT JOIN [State] s ON p.StateId = s.StateId
        LEFT JOIN Postal z ON p.PostalId = z.PostalId
        LEFT JOIN ExperienceLevel el ON p.ExperienceLevelId = el.ExperienceLevelId
        LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
        LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
        LEFT JOIN profileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
        WHERE co.CompanyName = ''Public Data''
        AND p.IsDeleted = 0
    ), companyProfiles AS (
        -- company profiles contains the company-specific data that is populated by recruiters of that company
        SELECT p.IsWillingToRelocate, p.ProfileGuid, co.CompanyGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, NULL PublicSkills, ps.Skills PrivateSkills, pet.EmploymentTypes
        , dbo.fn_GetGeographyCoordinate(p.CityId, p.StateId, p.PostalId) [Location]
        , dbo.fn_GreatestDate(ISNULL(p.ModifyDate, p.CreateDate), pt.ModifyDate, ps.ModifyDate, default, default) ModifyDate, p.AzureIndexStatusId
        FROM G2.Profiles p
        INNER JOIN Subscriber su ON p.SubscriberId = su.SubscriberId
        INNER JOIN dbo.Company co ON p.CompanyId = co.CompanyId
        LEFT JOIN G2.ContactTypes ct ON p.ContactTypeId = ct.ContactTypeId
        LEFT JOIN City c ON p.CityId = c.CityId
        LEFT JOIN [State] s ON p.StateId = s.StateId
        LEFT JOIN Postal z ON p.PostalId = z.PostalId
        LEFT JOIN ExperienceLevel el ON p.ExperienceLevelId = el.ExperienceLevelId
        LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
        LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
        LEFT JOIN profileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
        WHERE co.CompanyName <> ''Public Data''
        AND p.IsDeleted = 0
    )
    -- subscriber data is prioritized in the following way: company specific first (if it exists), then public data second (if that exists), and finally subscriber-curated data
    SELECT sp.CreateDate, sp.PartnerGuid, sp.AvatarUrl, sp.SubscriberGuid, cp.ProfileGuid, cp.CompanyGuid
        , COALESCE(cp.FirstName, pp.FirstName, sp.FirstName) FirstName
        , COALESCE(cp.LastName, pp.LastName, sp.LastName) LastName
        , COALESCE(cp.Email, pp.Email, sp.Email) Email
        , COALESCE(cp.PhoneNumber, pp.PhoneNumber, sp.PhoneNumber) PhoneNumber
        , COALESCE(cp.ContactType, pp.ContactType) ContactType
        , COALESCE(cp.StreetAddress, pp.StreetAddress, sp.StreetAddress) StreetAddress
        , COALESCE(cp.City, pp.City, sp.City) City
        , COALESCE(cp.[State], pp.[State], sp.[State]) [State]
        , COALESCE(cp.Postal, pp.Postal, sp.Postal) Postal 
        , COALESCE(cp.ExperienceLevel, pp.ExperienceLevel) ExperienceLevel 
        , COALESCE(cp.EmploymentTypes, pp.EmploymentTypes) EmploymentTypes
        , COALESCE(cp.Title, pp.Title, sp.Title) Title
        , COALESCE(cp.IsWillingToTravel, pp.IsWillingToTravel) IsWillingToTravel
        , COALESCE(cp.IsActiveJobSeeker, pp.IsActiveJobSeeker) IsActiveJobSeeker
        , COALESCE(cp.IsCurrentlyEmployed , pp.IsCurrentlyEmployed) IsCurrentlyEmployed
        , COALESCE(cp.IsWillingToWorkProBono, pp.IsWillingToWorkProBono) IsWillingToWorkProBono
        , COALESCE(cp.CurrentRate, pp.CurrentRate, sp.CurrentRate) CurrentRate
        , COALESCE(cp.DesiredRate, pp.DesiredRate) DesiredRate
        , COALESCE(cp.Tags, pp.Tags) Tags
        , COALESCE(pp.PublicSkills, sp.SubscriberSkills) PublicSkills
        , cp.PrivateSkills
        , COALESCE(cp.[Location], pp.[Location], sp.[Location]) [Location]
        , dbo.fn_GreatestDate(cp.ModifyDate, pp.ModifyDate, sp.ModifyDate, default, default) ModifyDate
        , cp.AzureIndexStatusId AzureIndexStatusId
        , COALESCE(cp.IsWillingToRelocate , pp.IsWillingToRelocate) IsWillingToRelocate
    FROM subscriberProfiles sp
    LEFT JOIN publicProfiles pp ON sp.SubscriberId = pp.SubscriberId
    LEFT JOIN companyProfiles cp ON sp.SubscriberId = cp.SubscriberId
')");

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
2020.06.04 - Bill Koenig - Exclude hiring managers from G2 search index
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
		LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
        WHERE c.IsDeleted = 0
    	AND C.CompanyId IN (
    		select c.companyId 
        	from recruiter r
        	inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
        	inner join Company c on rc.CompanyId = c.CompanyId
        	inner join Subscriber s on r.SubscriberId = s.SubscriberId
        	where s.IsDeleted = 0 
    	)
		AND hm.HiringManagerId IS NULL
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
2020.05.04 - Bill Koenig - Removed subscriber data from default configuration
2020.06.04 - Bill Koenig - Exclude hiring managers from G2 search index
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
        ,null as FirstName
        ,SUBSTRING(s.Email, 1, 254) Email
        ,null as PhoneNumber
        ,null as ContactTypeId
        ,null as StreetAddress
        ,null as CityId
        ,s.StateId
        ,null as PostalId
        ,null as ExperienceLevelId
        ,null as Title
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
        LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
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
		AND hm.HiringManagerId IS NULL
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
2020.05.04 - Bill Koenig - Removed subscriber data from default configuration
2020.06.04 - Bill Koenig - Exclude hiring managers from G2 search index
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
        ,null as FirstName
        ,SUBSTRING(s.Email, 1, 254) Email
        ,null as PhoneNumber
        ,null as ContactTypeId
        ,null as StreetAddress
        ,null as CityId
        ,null as StateId
        ,null as PostalId
        ,null as ExperienceLevelId
        ,null as Title
        ,0 as IsWillingToTravel
        ,0 as IsActivejobSeeker
        ,0 as IsCurrentlyEmployed
        ,0 as IsWillingToWorkProBono
        ,0 as CurrentRate
        ,0 as DesiredRate
        ,null as Goals
        ,null as Preferences
        FROM Subscriber  s
		LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
        LEFT JOIN Company c on c.CompanyGuid = @CompanyGuid
        WHERE s.SubscriberId NOT IN ( Select SubscriberId from g2.Profiles p where p.CompanyId = (select companyid from company where CompanyGuid = @CompanyGuid) ) and s.IsDeleted = 0
		AND hm.SubscriberId IS NULL
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
