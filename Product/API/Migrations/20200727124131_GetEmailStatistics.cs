using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class GetEmailStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.07.27 - Joey Herrington - Created
</remarks>
<description>
Get''s a list of sendgrid events per email by email address
</description>
<example>
EXEC [dbo].[System_Get_SendgridEvents] @emailAddress = ''bferree@careercircle.com'', @startDate = ''2020-06-27''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_SendgridEvents] (
	@emailAddress NVARCHAR(500),
	@startDate DATETIME
)
AS
BEGIN
	WITH MessageIds_CTE AS (
		SELECT DISTINCT e.Sg_message_id MessageId
		FROM dbo.SendGridEvent e
		WHERE e.Email = @emailAddress AND e.TimeStamp >= DATEDIFF(SECOND, ''1970-01-01'', @startDate)
	),
	Messages_CTE AS (
		SELECT
			e.Sg_message_id MessageId,
			e.Subject,
			DATEADD(SECOND, e.[TimeStamp], ''1970-01-01'') [TimeStamp],
			e.Event
		FROM dbo.SendGridEvent e
		WHERE e.Sg_message_id IN (SELECT * FROM MessageIds_CTE)
	)
	SELECT * FROM Messages_CTE
	PIVOT (
		MAX([TIMESTAMP])
		FOR [Event] IN (
			dropped,
			processed,
			click,
			delivered,
			bounce,
			deferred,
			[open]
		)
	) AS MessageEvents;
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_Get_SendgridEvents]");
        }
    }
}
