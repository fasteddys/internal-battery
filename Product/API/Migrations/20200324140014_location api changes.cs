using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class locationapichanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
2020.03.23 - Bill Koenig - Added CountryGuid to procedure result
</remarks>
<description>
Retrieves states for a country
</description>
<example>
EXEC [dbo].[System_Get_States] @Country = ''8B5DEC9A-B5CF-4BDC-B015-CCFD4339D32B'', @Limit = 10, @Offset = 0, @Sort = ''sequence'', @Order = ''ascending''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_States] (
    @Country uniqueidentifier,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT StateId
    	FROM [State] s 		
    	INNER JOIN Country c ON s.CountryId = c.CountryId
    	WHERE s.IsDeleted = 0
    	AND c.CountryGuid = @Country
    )
    SELECT StateGuid
        , [Name]
    	, Code
    	, s.[Sequence]
		, c.CountryGuid
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM [State] s 		
    INNER JOIN Country c ON s.CountryId = c.CountryId
    WHERE s.IsDeleted = 0
    AND c.CountryGuid = @Country
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN s.[Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''code'' THEN Code END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''sequence'' THEN s.[Sequence] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN s.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN s.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN s.[Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''code'' THEN Code END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''sequence'' THEN s.[Sequence] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN s.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN s.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.24 - Bill Koenig - Created
</remarks>
<description>
Retrieves cities for a state
</description>
<example>
EXEC [dbo].[System_Get_Cities] @State = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC'', @Limit = 100, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Cities] (
    @State uniqueidentifier,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT CityId
    	FROM City c 		
    	INNER JOIN [State] s ON c.StateId = s.StateId
    	WHERE c.IsDeleted = 0
    	AND s.StateGuid = @State
    )
    SELECT CityGuid
	, c.[Name]
	, s.StateGuid
	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM City c 		
    INNER JOIN [State] s ON c.StateId = s.StateId
    WHERE c.IsDeleted = 0
    AND s.StateGuid = @State   
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN c.[Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN c.[Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN c.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.24 - Bill Koenig - Created
</remarks>
<description>
Retrieves postals for a city
</description>
<example>
EXEC [dbo].[System_Get_Postals] @City = ''3BA0F582-694B-4198-9673-C47ED26A014B'', @Limit = 100, @Offset = 0, @Sort = ''code'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Postals] (
    @City uniqueidentifier,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT PostalId
    	FROM Postal p
		INNER JOIN City c ON p.CityId = c.CityId
    	WHERE p.IsDeleted = 0
    	AND c.CityGuid = @City
    )
    SELECT PostalGuid
	, p.Code
	, p.Latitude
	, p.Longitude
	, c.CityGuid
	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Postal p
	INNER JOIN City c ON p.CityId = c.CityId
    WHERE p.IsDeleted = 0
    AND c.CityGuid = @City 
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''code'' THEN p.Code END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN p.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN p.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''code'' THEN p.[Code] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN p.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN p.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
