using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class b2breportupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.06.05 - Bill Koenig - Created
2020.06.11 - Bill Koenig - Including additional B2B properties in output
</remarks>
<description>
This is used by the new API endpoint titled ''Dashboard - All Hiring Managers Detail Report''. This endpoint is intended to supply data to a 
downloadable CSV file in the admin portal which contains detailed information for all hiring managers.
</description>
<example>
EXEC [dbo].[System_Report_AllHiringManagersDetail]
</example>
*/
ALTER PROCEDURE [dbo].[System_Report_AllHiringManagersDetail]
AS
BEGIN
    ;WITH subscriberGroupPartners AS (
    	SELECT sg.SubscriberId, g.[Name] GroupName, p.[Name] PartnerName, ROW_NUMBER() OVER(PARTITION BY sg.SubscriberId ORDER BY sg.CreateDate ASC) RowNumOverSubscriberByCreateDateAsc
    	FROM SubscriberGroup sg
    	INNER JOIN [Group] g ON sg.GroupId = g.GroupId
    	LEFT JOIN GroupPartner gp ON g.GroupId = gp.GroupId
    	LEFT JOIN [Partner] p ON gp.PartnerId = p.PartnerId
    ), firstSourceAttribution AS (
    	SELECT SubscriberId, GroupName, PartnerName
    	FROM subscriberGroupPartners
    	WHERE RowNumOverSubscriberByCreateDateAsc = 1
    )
    SELECT s.SubscriberGuid
    	, s.CreateDate
    	, s.FirstName
    	, s.LastName
    	, s.Email
    	, s.PhoneNumber
    	, s.City
    	, p.Code [State]
    	, s.PostalCode
		, s.Title
		, c.CompanyName
		, c.EmployeeSize
		, c.WebsiteUrl
		, hm.HardToFindFillSkillsRoles
		, hm.SkillsRolesWeAreAlwaysHiringFor
		, i.[Name] IndustryName
    	, fsa.PartnerName
    	, fsa.GroupName
    FROM Subscriber s
	INNER JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
	LEFT JOIN dbo.Company c ON hm.CompanyId = c.CompanyId
	LEFT JOIN dbo.Industry i ON c.IndustryId = i.IndustryId
    LEFT JOIN [State] p on s.StateId = p.StateId
    LEFT JOIN firstSourceAttribution fsa ON fsa.SubscriberId = s.SubscriberId 
    WHERE s.IsDeleted = 0  
	AND hm.IsDeleted = 0
    ORDER BY s.CreateDate DESC          
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
