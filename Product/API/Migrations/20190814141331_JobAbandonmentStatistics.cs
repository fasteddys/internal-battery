using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class JobAbandonmentStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC('
                 CREATE PROCEDURE [dbo].[System_JobAbandonmentStatistics] 
                    @StartDate DATETIME
                    ,@EndDate DATETIME
                AS
                BEGIN
                SELECT query.ActionCreateDate 
                    ,Count(SubscriberId) AS Count
                FROM (
                    SELECT DISTINCT a.subscriberId AS SubscriberId
                        ,su.FirstName
                        ,su.Email
                        ,su.LastName
                        ,ja.JobApplicationId
                        ,Convert(DATE, a.CreateDate)  AS ActionCreateDate
                        ,ja.CreateDate
                    FROM SubscriberAction a
                    JOIN Subscriber su ON a.SubscriberId = su.SubscriberId
                    JOIN JobPosting jp ON jp.JobPostingId = a.EntityId
                    LEFT JOIN JobApplication ja ON ja.JobPostingId = jp.JobPostingId
                        AND ja.SubscriberId = a.SubscriberId
                    WHERE a.actionId = 9
                        AND a.CreateDate >= @StartDate
                        AND a.CreateDate <= @EndDate
                        AND (
                            (
                                ja.CreateDate IS NOT NULL
                                AND Convert(DATE, ja.CreateDate) >= DATEADD(day, 1, Convert(DATE, a.CreateDate))
                                )
                            OR (ja.CreateDate IS NULL)
                            )
                    ) AS query
                GROUP BY query.ActionCreateDate
                ORDER BY ActionCreateDate desc
                END
                ')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" DROP PROCEDURE [dbo].[System_JobAbandonmentStatistics]");
        }
    }
}
