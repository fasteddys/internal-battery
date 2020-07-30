using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Tweak_SendgridEventsSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.07.27 - Joey Herrington - Created
2020.07.30 - Joey Herrington - Added support for sorting, ordering, and pagnation
</remarks>
<description>
Get''s a list of sendgrid events per email by email address
</description>
<example>
DECLARE @totalCount INT
EXEC dbo.System_Get_SendgridEvents
	@emailAddress = ''bferree@careercircle.com'',
	@startDate = ''2020-06-27'',
	@sort = ''ascending'',
	@order = ''MessageId'',
	@limit = 1000,
	@offset = 0,
	@totalRecords = @totalRecords OUTPUT;
SELECT @totalCount;
</example>
*/
ALTER PROCEDURE dbo.System_Get_SendgridEvents (
	@emailAddress NVARCHAR(500),
	@startDate DATETIME,
	@sort NVARCHAR(500),
	@order NVARCHAR(500),
	@limit INT,
	@offset INT,
	@totalRecords INT OUTPUT
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
	SELECT
		*
	INTO #sendGridTempTable
	FROM Messages_CTE
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

	SELECT @totalRecords = COUNT(*) FROM #sendGridTempTable;

	SELECT *
	FROM #sendGridTempTable
	ORDER BY
		CASE WHEN @sort = ''MessageId'' AND @order = ''ascending'' THEN MessageId END,
		CASE WHEN @sort = ''MessageId'' AND @order = ''descending'' THEN MessageId END DESC,
		CASE WHEN @sort = ''Subject'' AND @order = ''ascending'' THEN [Subject] END,
		CASE WHEN @sort = ''Subject'' AND @order = ''descending'' THEN [Subject] END DESC,
		CASE WHEN @sort = ''dropped'' AND @order = ''ascending'' THEN dropped END,
		CASE WHEN @sort = ''dropped'' AND @order = ''descending'' THEN dropped END DESC,
		CASE WHEN @sort = ''processed'' AND @order = ''ascending'' THEN processed END,
		CASE WHEN @sort = ''processed'' AND @order = ''descending'' THEN processed END DESC,
		CASE WHEN @sort = ''click'' AND @order = ''ascending'' THEN click END,
		CASE WHEN @sort = ''click'' AND @order = ''descending'' THEN click END DESC,
		CASE WHEN @sort = ''delivered'' AND @order = ''ascending'' THEN delivered END,
		CASE WHEN @sort = ''delivered'' AND @order = ''descending'' THEN delivered END DESC,
		CASE WHEN @sort = ''bounce'' AND @order = ''ascending'' THEN bounce END,
		CASE WHEN @sort = ''bounce'' AND @order = ''descending'' THEN bounce END DESC,
		CASE WHEN @sort = ''deferred'' AND @order = ''ascending'' THEN deferred END,
		CASE WHEN @sort = ''deferred'' AND @order = ''descending'' THEN deferred END DESC,
		CASE WHEN @sort = ''open'' AND @order = ''ascending'' THEN [open] END,
		CASE WHEN @sort = ''open'' AND @order = ''descending'' THEN [open] END DESC
	OFFSET @offset ROWS
	FETCH FIRST @limit ROWS ONLY;

	DROP table #sendGridTempTable;

END')");

		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
