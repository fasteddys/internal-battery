using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class admindashboardreportingfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.08.26 - Bhupesh Vipparla - Created
2019.09.20 - Bill Koenig - Added support for nullable date range, fixed issue that was preventing partners without associated 
    subscribers from being displayed, start and end date range now references a floored Subscriber CreateDate rather than the	
    SubscriberGroup CreateDate, simplified overall logic, renamed stored procedure to match existing pattern (including 
    the model to which the result is being bound) , and added comment block to beginning of stored procedure.
2019.09.27 - Brent Ferree - I changed this to a subselect vs a join to ensure I only obtained the earliest row for SubscriberGroup
</remarks>
<description>
Returns partner attribution data for subscribers and enrollments.
</description>
<example>
DECLARE @StartDate DATETIME = ''10/22/2018''
DECLARE @EndDate DATETIME = NULL

EXEC [dbo].[System_Get_SubscriberSignUpCourseEnrollmentStatistics] 
    @StartDate = @StartDate
    , @EndDate = @EndDate
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberSignUpCourseEnrollmentStatistics]  (
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
)
AS
BEGIN
    ;WITH filteredSubscriberGroup AS (
		SELECT s.SubscriberId, (select top 1 GroupId from SubscriberGroup where SubscriberGroup.SubscriberId = s.SubscriberId and SubscriberGroup.IsDeleted=0 order by SubscriberGroup.CreateDate) GroupId
    	FROM Subscriber s
    	WHERE s.IsDeleted = 0 
    	AND (DATEADD(DAY, DATEDIFF(DAY, 0, s.CreateDate), 0) >= @StartDate OR @StartDate IS NULL) 
    	AND	(DATEADD(DAY, DATEDIFF(DAY, 0, s.CreateDate), 0) <= @EndDate OR @EndDate IS NULL)
    ),
    groupPartnersWithSubscribers AS (
    SELECT p.[Name] [PartnerName]
    	, g.[Name] [GroupName]
    	, (
    		SELECT COUNT(1) 
    		FROM filteredSubscriberGroup sg 
    		WHERE sg.GroupId = g.GroupId) [SubscriberCount]
    	, (
    		SELECT COUNT(1) 
    		FROM filteredSubscriberGroup sg 
    		INNER JOIN Enrollment e ON sg.SubscriberId = e.SubscriberId 
    		WHERE e.IsDeleted = 0 AND sg.GroupId = g.GroupId) [EnrollmentCount]
    FROM GroupPartner gp
    INNER JOIN [Group] g on gp.GroupId = g.GroupId
    INNER JOIN [Partner] p on gp.PartnerId = p.PartnerId
    WHERE g.IsDeleted = 0
    AND p.IsDeleted = 0
    AND gp.IsDeleted = 0
    GROUP BY p.[Name], g.[Name], g.GroupId
    )
    SELECT PartnerName
    	, SUM(SubscriberCount) [SubscriberCount] 
    	, SUM(EnrollmentCount) [EnrollmentCount] 
    from groupPartnersWithSubscribers
    GROUP BY PartnerName
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.08.26 - Bhupesh Vipparla - Created
2019.09.20 - Bill Koenig - Added support for nullable date range, fixed issue that was preventing partners without associated 
	subscribers from being displayed, start and end date range now references a floored Subscriber CreateDate rather than the	
	SubscriberGroup CreateDate, flooresimplified overall logic, renamed stored procedure to match existing pattern (including 
	the model to which the result is being bound) , and added comment block to beginning of stored procedure.
</remarks>
<description>
Returns partner attribution data for subscribers and enrollments.
</description>
<example>
DECLARE @StartDate DATETIME = ''10/22/2018''
DECLARE @EndDate DATETIME = NULL

EXEC [dbo].[System_Get_SubscriberSignUpCourseEnrollmentStatistics] 
	@StartDate = @StartDate
	, @EndDate = @EndDate
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberSignUpCourseEnrollmentStatistics]  (
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
)
AS
BEGIN
	;WITH filteredSubscriberGroup AS (
		SELECT sg.SubscriberId, sg.GroupId
		FROM SubscriberGroup sg
		INNER JOIN Subscriber s ON sg.SubscriberId = s.SubscriberId
		WHERE s.IsDeleted = 0
		AND sg.IsDeleted = 0
		AND (DATEADD(DAY, DATEDIFF(DAY, 0, s.CreateDate), 0) >= @StartDate OR @StartDate IS NULL) 
		AND	(DATEADD(DAY, DATEDIFF(DAY, 0, s.CreateDate), 0) <= @EndDate OR @EndDate IS NULL)
	),
	groupPartnersWithSubscribers AS (
	SELECT p.[Name] [PartnerName]
		, g.[Name] [GroupName]
		, (
			SELECT COUNT(1) 
			FROM filteredSubscriberGroup sg 
			WHERE sg.GroupId = g.GroupId) [SubscriberCount]
		, (
			SELECT COUNT(1) 
			FROM filteredSubscriberGroup sg 
			INNER JOIN Enrollment e ON sg.SubscriberId = e.SubscriberId 
			WHERE e.IsDeleted = 0 AND sg.GroupId = g.GroupId) [EnrollmentCount]
	FROM GroupPartner gp
	INNER JOIN [Group] g on gp.GroupId = g.GroupId
	INNER JOIN [Partner] p on gp.PartnerId = p.PartnerId
	WHERE g.IsDeleted = 0
	AND p.IsDeleted = 0
	AND gp.IsDeleted = 0
	GROUP BY p.[Name], g.[Name], g.GroupId
	)
	SELECT PartnerName
		, SUM(SubscriberCount) [SubscriberCount] 
		, SUM(EnrollmentCount) [EnrollmentCount] 
	from groupPartnersWithSubscribers
	GROUP BY PartnerName

END
            ')");
        }
    }
}
