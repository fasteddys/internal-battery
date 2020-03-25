using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedReportSPEndDatesfromgttogtorequals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
    <remarks>
    2020.01.30 - Bill Koenig - Created
    </remarks>
    <description>
    This is used by the new API endpoint titled 'Dashboard - New Users Report'. This endpoint is intended to supply data to a 
    report in the admin portal that displays aggregate data on user creation and course enrollment week over week for the last year.
    </description>
    <example>
    EXEC [dbo].[System_Report_NewUsers]
    </example>
    */
    ALTER PROCEDURE [dbo].[System_Report_NewUsers]
    AS
    BEGIN
    	DECLARE @maxLookbackInDays INT = 365;
    	DECLARE @startDate DATETIME = DATEADD(DAY, -1 * @maxLookbackInDays, GETUTCDATE());
    	DECLARE @firstMondayAfterStartDate DATETIME = DATEADD(DAY, DATEDIFF(DAY, 0, @startDate - 1) / 7 * 7, 0) + 7;

    	;WITH startDates AS (
    		SELECT StartDate = @firstMondayAfterStartDate
    		UNION ALL
    		SELECT DATEADD(DAY, 7, startDate)
    		FROM startDates
    		WHERE DATEADD(DAY, 7, startDate) < GETUTCDATE()
    	), totals AS (
    		SELECT 
    			(SELECT COUNT(1)
    			FROM Subscriber
    			WHERE IsDeleted = 0) TotalUsers
    			, (SELECT COUNT(1) 
    			FROM Enrollment 
    			WHERE IsDeleted = 0) TotalEnrollments
    	)
    	SELECT CONVERT(VARCHAR(10), d.StartDate, 101) + ' - ' + CONVERT(VARCHAR(10), DATEADD(DAY, 7, d.StartDate), 101) DateRange
    		, COUNT(DISTINCT s.SubscriberId) UsersCreated
    		, COUNT(DISTINCT e.EnrollmentId) EnrollmentsCreated
    		, (SELECT TOP 1 TotalUsers FROM totals) TotalUsers
    		, (SELECT TOP 1 TotalEnrollments FROM totals) TotalEnrollments
    	FROM startDates d
    	LEFT JOIN dbo.Subscriber s ON s.CreateDate >= d.startDate AND s.CreateDate <= DATEADD(DAY, 7, d.startDate) AND s.IsDeleted = 0
    	LEFT JOIN dbo.Enrollment e ON e.CreateDate >= d.startDate AND e.CreateDate <= DATEADD(DAY, 7, d.startDate) AND e.IsDeleted = 0
    	GROUP BY d.startDate 
    	ORDER BY StartDate DESC
    END')");

            migrationBuilder.Sql(@"EXEC('/*
    <remarks>
    2020.01.30 - Bill Koenig - Created
    </remarks>
    <description>
    This is used by the new API endpoint titled 'Dashboard - Users By Partner Report'. This endpoint is intended to supply data to a
    report in the admin portal that displays aggregate data on user creation and course enrollment by partner. 
    </description>
    <example>
    EXEC [dbo].[System_Report_UsersByPartner] @StartDate = '10/1/2019', @EndDate = '1/1/2020'
    </example>
    */
    ALTER PROCEDURE [dbo].[System_Report_UsersByPartner](
        @StartDate DATETIME,
    	@EndDate DATETIME
    )
    AS
    BEGIN 
    	;WITH subscriberPartners AS (
    		SELECT s.SubscriberId, p.PartnerId, ROW_NUMBER() OVER(PARTITION BY sg.SubscriberId ORDER BY sg.CreateDate ASC) RowNumOverSubscriberByCreateDateAsc
    		FROM Subscriber s
    		INNER JOIN SubscriberGroup sg ON s.SubscriberId = sg.SubscriberId
    		INNER JOIN [Group] g ON sg.GroupId = g.GroupId
    		INNER JOIN GroupPartner gp ON g.GroupId = gp.GroupId
    		INNER JOIN [Partner] p ON gp.PartnerId = p.PartnerId
    		WHERE s.IsDeleted = 0
    		AND s.CreateDate >= @StartDate AND s.CreateDate <= @EndDate
    	), firstSourceSubscriberPartners AS (
    		SELECT SubscriberId, PartnerId
    		FROM subscriberPartners 
    		WHERE RowNumOverSubscriberByCreateDateAsc = 1
    	), enrollmentPartners AS (
    		SELECT e.EnrollmentId, s.SubscriberId, p.PartnerId, ROW_NUMBER() OVER(PARTITION BY sg.SubscriberId ORDER BY sg.CreateDate ASC) RowNumOverSubscriberByCreateDateAsc
    		FROM Subscriber s
    		INNER JOIN Enrollment e ON s.SubscriberId = e.SubscriberId
    		INNER JOIN SubscriberGroup sg ON s.SubscriberId = sg.SubscriberId
    		INNER JOIN [Group] g ON sg.GroupId = g.GroupId
    		INNER JOIN GroupPartner gp ON g.GroupId = gp.GroupId
    		INNER JOIN [Partner] p ON gp.PartnerId = p.PartnerId
    		WHERE s.IsDeleted = 0
    		AND e.IsDeleted = 0
    		AND e.CreateDate >= @StartDate AND e.CreateDate <= @EndDate
    	), firstSourceEnrollmentPartners AS (
    		SELECT EnrollmentId, SubscriberId, PartnerId
    		FROM enrollmentPartners
    		WHERE RowNumOverSubscriberByCreateDateAsc = 1
    	)
    	SELECT p.PartnerGuid
    		, p.[Name] PartnerName
    		, (SELECT COUNT(DISTINCT SubscriberId) FROM firstSourceSubscriberPartners fssp WHERE fssp.PartnerId = p.PartnerId) UsersCreated
    		, (SELECT COUNT(DISTINCT EnrollmentId) FROM firstSourceEnrollmentPartners fsep WHERE fsep.PartnerId = p.PartnerId) EnrollmentsCreated
    	FROM [Partner] p
    	WHERE IsDeleted = 0
    END')");


            migrationBuilder.Sql(@"EXEC('/*
    <remarks>
    2020.01.30 - Bill Koenig - Created
    </remarks>
    <description>
    This is used by the new API endpoint titled 'Dashboard - Users By Partner Detail Report'. This endpoint is intended to supply 
    data to a report in the admin portal that displays detailed data on user creation and course enrollment for the supplied partner.
    </description>
    <example>
    EXEC [dbo].[System_Report_UsersByPartnerDetail] @Partner = '88AB14F3-D2BF-4458-BEE2-41F7C732274B', @StartDate = '10/1/2019', @EndDate = '1/1/2020'
    </example>
    */
    ALTER PROCEDURE [dbo].[System_Report_UsersByPartnerDetail](
        @Partner UNIQUEIDENTIFIER,
    	@StartDate DATETIME,
    	@EndDate DATETIME
    )
    AS
    BEGIN 
    	;WITH attribution AS (
    		SELECT sg.SubscriberGroupId, sg.CreateDate, sg.SubscriberId, g.[Name] GroupName, p.PartnerGuid, p.[Name] PartnerName, ROW_NUMBER() OVER(PARTITION BY sg.SubscriberId ORDER BY sg.CreateDate ASC) RowNumOverSubscriberByCreateDateAsc
    		FROM SubscriberGroup sg
    		INNER JOIN [Group] g ON sg.GroupId = g.GroupId
    		INNER JOIN GroupPartner gp ON g.GroupId = gp.GroupId
    		INNER JOIN [Partner] p ON gp.PartnerId = p.PartnerId
    	), firstSourceAttribution AS (
    		SELECT a.SubscriberId, a.PartnerGuid, a.PartnerName, a.GroupName
    		FROM attribution a
    		WHERE a.RowNumOverSubscriberByCreateDateAsc = 1
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
    	LEFT JOIN [State] p on s.StateId = p.StateId
    	INNER JOIN firstSourceAttribution fsa ON fsa.SubscriberId = s.SubscriberId AND fsa.PartnerGuid = @Partner
    	WHERE s.IsDeleted = 0  
    	AND s.CreateDate >= @StartDate
    	AND s.CreateDate <= @EndDate
    	ORDER BY s.CreateDate DESC   
    END')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
