using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class recruiterstats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruiterStat",
                columns: table => new
                {
                    RecruiterStatId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    OpCoSubmittals = table.Column<int>(nullable: false),
                    CCSubmittals = table.Column<int>(nullable: false),
                    OpCoInterviews = table.Column<int>(nullable: false),
                    CCInterviews = table.Column<int>(nullable: false),
                    OpCoStarts = table.Column<int>(nullable: false),
                    CCStarts = table.Column<int>(nullable: false),
                    OpCoSpread = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CCSpread = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterStat", x => x.RecruiterStatId);
                });

            migrationBuilder.Sql(@"EXEC('
DECLARE @startDate DATETIME = ''1/1/2020''
DECLARE @endDate DATETIME = ''1/1/2022''
DECLARE @firstMondayAfterStartDate DATETIME = DATEADD(DAY, DATEDIFF(DAY, 0, @startDate - 1) / 7 * 7, 0) + 7;

;WITH startDates AS (
	SELECT StartDate = @firstMondayAfterStartDate
	UNION ALL
	SELECT DATEADD(DAY, 7, startDate)
	FROM startDates
	WHERE DATEADD(DAY, 7, startDate) < @endDate
)
INSERT INTO dbo.RecruiterStat(StartDate, EndDate, OpCoSubmittals, CCSubmittals, OpCoInterviews, CCInterviews, OpCoStarts, CCStarts, OpCoSpread, CCSpread)
SELECT StartDate, DATEADD(DAY, 7, StartDate) EndDate, 0 OpCoSubmittals, 0 CCSubmittals, 0 OpCoInterviews, 0 CCInterviews, 0 OpCoStarts, 0 CCStarts, 0 OpCoSpread, 0 CCSpread
FROM startDates
OPTION (MAXRECURSION 500)
')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.11 - Bill Koenig - Created
</remarks>
<description>
Returns recruiter stats 
</description>
<example>
EXEC [dbo].[System_Get_RecruiterStats] @Year = 2020
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_RecruiterStats] (
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
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.System_Get_RecruiterStats");

            migrationBuilder.DropTable(
                name: "RecruiterStat");
        }
    }
}
