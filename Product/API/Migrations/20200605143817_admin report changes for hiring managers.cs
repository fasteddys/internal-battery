using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class adminreportchangesforhiringmanagers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.11 - Bill Koenig - Created
2020.04.20 - Brent Ferree - Changed sort order (added to migration script 2020.06.05)
</remarks>
<description>
Returns recruiter stats 
</description>
<example>
EXEC [dbo].[System_Get_RecruiterStats] @Year = 2020
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_RecruiterStats] (
    @Year INT
)
AS
BEGIN
    WITH allRecords AS (
    	SELECT SUM(OpCoSubmittals) TotalOpCoSubmittals
    		, SUM(CCSubmittals) TotalCCSubmittals
    		, SUM(OpCoInterviews) TotalOpCoInterviews
    		, SUM(CCInterviews) TotalCCInterviews
    		, SUM(OpCoStarts) TotalOpCoStarts
    		, SUM(CCStarts) TotalCCStarts
    		, SUM(OpCoSpread) TotalOpCoSpread
    		, SUM(CCSpread) TotalCCSpread
    	FROM dbo.RecruiterStat
    	WHERE Year(EndDate) = @Year
    )
    SELECT RecruiterStatId
    	, CONVERT(VARCHAR(10), StartDate, 101) + '' - '' + CASE WHEN EndDate > GETUTCDATE() THEN ''Current'' ELSE CONVERT(VARCHAR(10), EndDate, 101) END DateRange
    	, OpCoSubmittals
    	, CCSubmittals
    	, OpCoInterviews
    	, CCInterviews
    	, OpCoStarts
    	, CCStarts
    	, OpCoSpread
    	, CCSpread
    	, (SELECT TOP 1 TotalOpCoSubmittals FROM allRecords) TotalOpCoSubmittals
    	, (SELECT TOP 1 TotalCCSubmittals FROM allRecords) TotalCCSubmittals
    	, (SELECT TOP 1 TotalOpCoInterviews FROM allRecords) TotalOpCoInterviews
    	, (SELECT TOP 1 TotalCCInterviews FROM allRecords) TotalCCInterviews
    	, (SELECT TOP 1 TotalOpCoStarts FROM allRecords) TotalOpCoStarts
    	, (SELECT TOP 1 TotalCCStarts FROM allRecords) TotalCCStarts
    	, (SELECT TOP 1 TotalOpCoSpread FROM allRecords) TotalOpCoSpread
    	, (SELECT TOP 1 TotalCCSpread FROM allRecords) TotalCCSpread
    FROM dbo.RecruiterStat
    WHERE YEAR(EndDate) = @Year
    AND StartDate < GETUTCDATE()
	ORDER BY EndDate DESC
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.30 - Bill Koenig - Created
2020.06.05 - Bill Koenig - Excluding hiring managers (a new report is being created for them)
</remarks>
<description>
This is used by the new API endpoint titled ''Dashboard - All Users Detail Report''. This endpoint is intended to supply data to a 
downloadable CSV file in the admin portal which contains detailed information for all users (excluding hiring managers).
</description>
<example>
EXEC [dbo].[System_Report_AllUsersDetail]
</example>
*/
ALTER PROCEDURE [dbo].[System_Report_AllUsersDetail]
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
    	, fsa.PartnerName
    	, fsa.GroupName
    	, (SELECT COUNT(1) FROM Enrollment e WHERE e.SubscriberId = s.SubscriberId AND e.IsDeleted = 0) [EnrollmentsCreated] 
    FROM Subscriber s
	LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
    LEFT JOIN [State] p on s.StateId = p.StateId
    LEFT JOIN firstSourceAttribution fsa ON fsa.SubscriberId = s.SubscriberId 
    WHERE s.IsDeleted = 0  
	AND hm.HiringManagerId IS NULL
    ORDER BY s.CreateDate DESC          
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.06.05 - Bill Koenig - Created
</remarks>
<description>
This is used by the new API endpoint titled ''Dashboard - All Hiring Managers Detail Report''. This endpoint is intended to supply data to a 
downloadable CSV file in the admin portal which contains detailed information for all hiring managers.
</description>
<example>
EXEC [dbo].[System_Report_AllHiringManagersDetail]
</example>
*/
CREATE PROCEDURE [dbo].[System_Report_AllHiringManagersDetail]
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
    	, fsa.PartnerName
    	, fsa.GroupName
    	, (SELECT COUNT(1) FROM Enrollment e WHERE e.SubscriberId = s.SubscriberId AND e.IsDeleted = 0) [EnrollmentsCreated] 
    FROM Subscriber s
	INNER JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
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
