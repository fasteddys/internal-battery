using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class g2view : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wishlists_RecruiterId",
                schema: "G2",
                table: "Wishlists");

            migrationBuilder.DropIndex(
                name: "IX_ProfileWishlists_ProfileId",
                schema: "G2",
                table: "ProfileWishlists");

            migrationBuilder.DropIndex(
                name: "IX_ProfileTags_TagId",
                schema: "G2",
                table: "ProfileTags");

            migrationBuilder.CreateIndex(
                name: "UIX_Wishlist_Recruiter_Name",
                schema: "G2",
                table: "Wishlists",
                columns: new[] { "RecruiterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_ProfileWishlist_Profile_Wishlist",
                schema: "G2",
                table: "ProfileWishlists",
                columns: new[] { "ProfileId", "WishlistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_ProfileTag_Profile_Tag",
                schema: "G2",
                table: "ProfileTags",
                columns: new[] { "TagId", "ProfileId" },
                unique: true);

            migrationBuilder.Sql(@"EXEC('
/*
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
CREATE VIEW [G2].[v_ProfileAzureSearch]
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_Wishlist_Recruiter_Name",
                schema: "G2",
                table: "Wishlists");

            migrationBuilder.DropIndex(
                name: "UIX_ProfileWishlist_Profile_Wishlist",
                schema: "G2",
                table: "ProfileWishlists");

            migrationBuilder.DropIndex(
                name: "UIX_ProfileTag_Profile_Tag",
                schema: "G2",
                table: "ProfileTags");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_RecruiterId",
                schema: "G2",
                table: "Wishlists",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileWishlists_ProfileId",
                schema: "G2",
                table: "ProfileWishlists",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTags_TagId",
                schema: "G2",
                table: "ProfileTags",
                column: "TagId");
        }
    }
}
