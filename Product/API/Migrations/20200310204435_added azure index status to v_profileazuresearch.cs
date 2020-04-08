using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedazureindexstatustov_profileazuresearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-21 - Bill Koenig - Created
2020-02-25 - Bill Koenig - Changed data type for current and desired rate, removed support for one to many search locations, added single location	column which returns ''geography'' data type, prioritized the selection of the geo data based on specificity (function), incorporated public and private profile into CTEs, included modify date that indicates the most recent change across all profiles for indexing operations, added code comments for readability
2020-02-26 - Bill Koenig - Primary CTE is now subscriberProfiles with LEFT JOINs to publicProfiles and companyProfiles so that every subscriber has a record whether or not they have a public profile (we still need to create companyProfiles for everyone though), added SubscriberGuid and CompanyGuid to the view output, added check for logical deletes on profiles
2020-03-10 - JAB - Added AzureIndexStatus 
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
		SELECT ss.SubscriberId, s.SkillName, ISNULL(ss.ModifyDate, ss.CreateDate) ModifyDate
		FROM SubscriberSkill ss
		INNER JOIN Skill s ON ss.SkillId = s.SkillId
		WHERE ss.IsDeleted = 0
		AND s.IsDeleted = 0
	), subscriberProfiles AS (
		-- returns a flattened view of all subscriber curated data
		SELECT s.SubscriberId, s.SubscriberGuid, CASE WHEN LEN(FirstName) > 0 THEN FirstName ELSE NULL END FirstName, CASE WHEN LEN(LastName) > 0 THEN LastName ELSE NULL END LastName, Email, CASE WHEN LEN(PhoneNumber) > 0 THEN PhoneNumber ELSE NULL END PhoneNumber, CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress, CASE WHEN LEN(City) > 0 THEN City ELSE NULL END City, t.Code [State], CASE WHEN LEN(PostalCode) > 0 THEN PostalCode ELSE NULL END Postal, mrswh.Title, CASE WHEN mrswh.CompensationTypeName = ''Hourly'' THEN mrswh.Compensation ELSE NULL END CurrentRate, STRING_AGG(ss.SkillName, '';'') SubscriberSkills
		, dbo.fn_GetGeographyCoordinate((SELECT TOP 1 CityId FROM dbo.City c INNER JOIN dbo.[State] t ON c.StateId = t.StateId WHERE c.[Name] = s.City AND t.StateId = s.StateId), s.StateId, (SELECT TOP 1 PostalId FROM dbo.Postal WHERE Code = s.PostalCode)) [Location]
		, dbo.fn_GreatestDate(ISNULL(s.ModifyDate, s.CreateDate), mrswh.ModifyDate, MAX(ss.ModifyDate), default, default) ModifyDate
		FROM Subscriber s	
		LEFT JOIN [State] t ON s.StateId = t.StateId
		LEFT JOIN mostRecentSubscriberWorkHistory mrswh ON s.SubscriberId = mrswh.SubscriberId
		LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId
		WHERE s.IsDeleted = 0
		GROUP BY s.SubscriberId, s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.[Address], s.City, t.Code, s.PostalCode, mrswh.Title, mrswh.CompensationTypeName, mrswh.Compensation, s.StateId, s.CreateDate, s.ModifyDate, mrswh.ModifyDate
	)
	, profileTags AS (
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
	), publicProfiles AS (
		-- public profiles contains the ''base'' profile image as well as data extracted from third party sources such as Connected
		SELECT p.ProfileGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, et.[Name] EmploymentType, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, ps.Skills PublicSkills, NULL PrivateSkills
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
		LEFT JOIN EmploymentType et ON p.EmploymentTypeId = et.EmploymentTypeId
		LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
		WHERE co.CompanyName = ''Public Data''
        AND p.IsDeleted = 0
	), companyProfiles AS (
		-- company profiles contains the company-specific data that is populated by recruiters of that company
		SELECT p.ProfileGuid, co.CompanyGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, et.[Name] EmploymentType, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, NULL PublicSkills, ps.Skills PrivateSkills
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
		LEFT JOIN EmploymentType et ON p.EmploymentTypeId = et.EmploymentTypeId
		LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
		WHERE co.CompanyName <> ''Public Data''
        AND p.IsDeleted = 0
	)
	-- subscriber data is prioritized in the following way: company specific first (if it exists), then public data second (if that exists), and finally subscriber-curated data
	SELECT sp.SubscriberGuid, cp.ProfileGuid, cp.CompanyGuid
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
		, COALESCE(cp.EmploymentType, pp.EmploymentType) EmploymentType
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
	  
	FROM subscriberProfiles sp
	LEFT JOIN publicProfiles pp ON sp.SubscriberId = pp.SubscriberId
	LEFT JOIN companyProfiles cp ON sp.SubscriberId = cp.SubscriberId')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-21 - Bill Koenig - Created
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
		SELECT swh.SubscriberId, swh.StartDate, swh.Title, swh.Compensation, ct.CompensationTypeName, ROW_NUMBER() OVER(PARTITION BY swh.SubscriberId ORDER BY swh.StartDate DESC) rownum
		FROM SubscriberWorkHistory swh
		LEFT JOIN CompensationType ct ON swh.CompensationTypeId = ct.CompensationTypeId
		WHERE swh.IsDeleted = 0
	), mostRecentSubscriberWorkHistory AS (
		SELECT SubscriberId, Title, Compensation, CompensationTypeName
		FROM subscriberWorkHistoryWithRownum
		WHERE rownum = 1
	), subscriberSkills AS (
		SELECT ss.SubscriberId, s.SkillName
		FROM SubscriberSkill ss
		INNER JOIN Skill s ON ss.SkillId = s.SkillId
		WHERE ss.IsDeleted = 0
		AND s.IsDeleted = 0
	), subscriberProfiles AS (
		SELECT s.SubscriberId, CASE WHEN LEN(FirstName) > 0 THEN FirstName ELSE NULL END FirstName, CASE WHEN LEN(LastName) > 0 THEN LastName ELSE NULL END LastName, Email, CASE WHEN LEN(PhoneNumber) > 0 THEN PhoneNumber ELSE NULL END PhoneNumber, CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress, CASE WHEN LEN(City) > 0 THEN City ELSE NULL END City, t.Code [State], CASE WHEN LEN(PostalCode) > 0 THEN PostalCode ELSE NULL END Postal, mrswh.Title, CASE WHEN mrswh.CompensationTypeName = ''Hourly'' THEN mrswh.Compensation ELSE NULL END CurrentRate, STRING_AGG(ss.SkillName, '';'') SubscriberSkills
		FROM Subscriber s	
		LEFT JOIN [State] t ON s.StateId = t.StateId
		LEFT JOIN mostRecentSubscriberWorkHistory mrswh ON s.SubscriberId = mrswh.SubscriberId
		LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId
		WHERE s.IsDeleted = 0
		GROUP BY s.SubscriberId, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.[Address], s.City, t.Code, s.PostalCode, mrswh.Title, mrswh.CompensationTypeName, mrswh.Compensation
	), profileTags AS (
		SELECT p.ProfileId, STRING_AGG(t.[Name], '';'') Tags
		FROM G2.Profiles p
		LEFT JOIN G2.ProfileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN dbo.Tag t ON pt.TagId = t.TagId
		WHERE pt.IsDeleted = 0
		GROUP BY p.ProfileId
	), profileSkills AS (
		SELECT p.ProfileId, STRING_AGG(s.SkillName, '';'') Skills
		FROM G2.Profiles p
		LEFT JOIN G2.ProfileSkills ps ON p.ProfileId = ps.ProfileId
		LEFT JOIN dbo.Skill s ON s.SkillId = ps.SkillId
		WHERE ps.IsDeleted = 0
		GROUP BY p.ProfileId
	), profileSearchLocations AS (
		SELECT ProfileId, STRING_AGG(SearchLocation, '';'') SearchLocations
		FROM (
			SELECT p.ProfileId, CONVERT(VARCHAR(20), CASE WHEN psl.PostalId IS NOT NULL THEN z.Latitude ELSE (SELECT TOP 1 Latitude FROM dbo.Postal WHERE CityId = c.CityId AND IsDeleted = 0) END) + ''|'' + CONVERT(VARCHAR(20), CASE WHEN psl.PostalId IS NOT NULL THEN z.Longitude ELSE (SELECT TOP 1 Longitude FROM dbo.Postal WHERE CityId = c.CityId AND IsDeleted = 0) END) + ''|'' + CONVERT(VARCHAR(10), psl.SearchRadius)
			FROM G2.Profiles p
			LEFT JOIN G2.ProfileSearchLocations psl ON p.ProfileId = psl.ProfileId
			INNER JOIN dbo.City c ON psl.CityId = c.CityId
			LEFT JOIN dbo.Postal z ON psl.PostalId = z.PostalId) psl([ProfileId],[SearchLocation])
		GROUP BY ProfileId
	)
	SELECT p.ProfileGuid
		, COALESCE(p.FirstName, sp.FirstName) FirstName
		, COALESCE(p.LastName, sp.LastName) LastName
		, COALESCE(p.Email, sp.Email) Email
		, COALESCE(p.PhoneNumber, sp.PhoneNumber) PhoneNumber
		, ct.[Name] ContactType
		, COALESCE(p.StreetAddress, sp.StreetAddress) StreetAddress
		, COALESCE(c.[Name], sp.City) City
		, COALESCE(s.Code, sp.[State]) [State]
		, COALESCE(z.Code, sp.Postal) Postal 
		, el.DisplayName ExperienceLevel 
		, et.[Name] EmploymentType
		, COALESCE(p.Title, sp.Title) Title
		, p.IsWillingToTravel 
		, p.IsActiveJobSeeker 
		, p.IsCurrentlyEmployed -- consider divining this based on work history from sp record?
		, p.IsWillingToWorkProBono
		, COALESCE(p.CurrentRate, sp.CurrentRate) CurrentRate
		, p.DesiredRate
		, pt.Tags
		, COALESCE(ps.Skills, sp.SubscriberSkills) Skills
		, psl.SearchLocations
	FROM G2.Profiles p
	INNER JOIN subscriberProfiles sp ON p.SubscriberId = sp.SubscriberId
	LEFT JOIN G2.ContactTypes ct ON p.ContactTypeId = ct.ContactTypeId
	LEFT JOIN City c ON p.CityId = c.CityId
	LEFT JOIN [State] s ON p.StateId = s.StateId
	LEFT JOIN Postal z ON p.PostalId = z.PostalId
	LEFT JOIN ExperienceLevel el ON p.ExperienceLevelId = el.ExperienceLevelId
	LEFT JOIN EmploymentType et ON p.EmploymentTypeId = et.EmploymentTypeId
	LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
	LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
	LEFT JOIN profileSearchLocations psl ON p.ProfileId = psl.ProfileId')");

        }
    }
}
