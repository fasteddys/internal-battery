using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class fixuserbreakdownbypartnerreport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
CREATE PROCEDURE [dbo].[System_Get_SubscriberSignUpCourseEnrollmentStatistics]  (
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

            migrationBuilder.Sql("DROP PROCEDURE dbo.System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
CREATE PROCEDURE [dbo].[System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner] 
@StartDate DATETIME
,@EndDate DATETIME
AS
BEGIN
                ;WITH 
                SignUp_CTE
                AS 
                (
                    Select p.Name As PartnerName, COUNT(*) AS SignUpCount, p.PartnerId
                    from Partner p
    				inner join GroupPartner gp on p.PartnerId=gp.PartnerId
    				inner join [Group] g on gp.GroupId=g.GroupId
    				inner join SubscriberGroup sg on g.GroupId=sg.GroupId
    				inner join Subscriber s on sg.SubscriberId=s.SubscriberId
                    where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0 and gp.IsDeleted=0 and sg.CreateDate >= @StartDate and sg.CreateDate <= @EndDate
                    Group by p.Name,p.PartnerId
                ), Enrollment_CTE AS
                (
                    Select p.Name As PartnerName, COUNT(*) AS Enrollments, p.PartnerId
                    from Partner p
    				inner join GroupPartner gp on p.PartnerId=gp.PartnerId
    				inner join [Group] g on gp.GroupId=g.GroupId
    				inner join SubscriberGroup sg on g.GroupId=sg.GroupId
    				inner join Subscriber s on sg.SubscriberId=s.SubscriberId
                    join Enrollment e on s.SubscriberId=e.SubscriberId
                    where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0 and e.IsDeleted=0 and gp.IsDeleted=0 and sg.CreateDate >= @StartDate and sg.CreateDate <= @EndDate
                    Group by p.PartnerId, p.Name
                )

            Select scte.PartnerName, ISNULL(scte.SignUpCount,0) as SubscriberCount,ISNULL(ecte.Enrollments,0) as EnrollmentCount 
            from SignUp_CTE scte
            Left join Enrollment_CTE ecte on scte.PartnerId=ecte.PartnerId
END
            ')");

            migrationBuilder.Sql("DROP PROCEDURE dbo.System_Get_SubscriberSignUpCourseEnrollmentStatistics");
        }
    }
}
