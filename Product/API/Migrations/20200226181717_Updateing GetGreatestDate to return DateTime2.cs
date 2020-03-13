using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class UpdateingGetGreatestDatetoreturnDateTime2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-25 - Bill Koenig - Created
2020-02-26 - Jim Brazil - Modified to return DateTime2
</remarks>
<description>
Returns the maximum datetime value from a list of two or more datetime values
</description>
<example>
SELECT [dbo].[fn_GreatestDate](''2019-11-01'', ''2020-01-01'', ''1970-01-01'', default, default)
SELECT [dbo].[fn_GreatestDate](''2019-11-01'', ''2020-01-01'', ''1970-01-01'', ''2021-10-31'', ''2029-03-17'')
</example>
*/
ALTER FUNCTION [dbo].[fn_GreatestDate](@date1 DATETIME2, @date2 DATETIME2, @date3 DATETIME2 = NULL, @date4 DATETIME2 = NULL, @date5 DATETIME2 = NULL)
RETURNS DATETIME2
AS
BEGIN

	DECLARE @tmp TABLE ([value] DATETIME2)
	INSERT INTO @tmp ([value]) VALUES(@date1)
	INSERT INTO @tmp ([value]) VALUES(@date2)
	IF(@date3 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date3)
	IF(@date4 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date4)
	IF(@date5 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date5)
	RETURN (SELECT MAX([value]) FROM @tmp);
END')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-25 - Bill Koenig - Created
</remarks>
<description>
Returns the maximum datetime value from a list of two or more datetime values
</description>
<example>
SELECT [dbo].[fn_GreatestDate](''2019-11-01'', ''2020-01-01'', ''1970-01-01'', default, default)
SELECT [dbo].[fn_GreatestDate](''2019-11-01'', ''2020-01-01'', ''1970-01-01'', ''2021-10-31'', ''2029-03-17'')
</example>
*/
CREATE FUNCTION [dbo].[fn_GreatestDate](@date1 DATETIME, @date2 DATETIME, @date3 DATETIME = NULL, @date4 DATETIME = NULL, @date5 DATETIME = NULL)
RETURNS DATETIME
AS
BEGIN

	DECLARE @tmp TABLE ([value] DATETIME)
	INSERT INTO @tmp ([value]) VALUES(@date1)
	INSERT INTO @tmp ([value]) VALUES(@date2)
	IF(@date3 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date3)
	IF(@date4 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date4)
	IF(@date5 IS NOT NULL)
		INSERT INTO @tmp ([value]) VALUES(@date5)
	RETURN (SELECT MAX([value]) FROM @tmp);
END')");

        }
    }
}
