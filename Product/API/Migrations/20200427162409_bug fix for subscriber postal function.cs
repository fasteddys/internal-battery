using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class bugfixforsubscriberpostalfunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE dbo.Subscriber DROP COLUMN [PostalGuid]");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-04-22 - Bill Koenig - Created
2020-04-26 - Bill Koenig - Bug fix; was returning CityGuid - now returning PostalGuid
</remarks>
<description>
Returns a PostalGuid that aligns with the city, state, and postal code for the subscriber. If a valid city, state, and postal combination has not been selected, no value will be returned.
</description>
<example>
SELECT [dbo].[fn_GetPostalGuidForSubscriber](''90210'', ''Bel Air'', NULL) -- invalid
SELECT [dbo].[fn_GetPostalGuidForSubscriber](''21015'', ''Bel Air'', 1570) -- valid 
</example>
*/
ALTER FUNCTION [dbo].[fn_GetPostalGuidForSubscriber](@PostalCode NVARCHAR(MAX), @CityName NVARCHAR(MAX), @StateId INT)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN

RETURN (SELECT TOP 1 p.PostalGuid
    	FROM dbo.Postal p
    	INNER JOIN dbo.City c ON c.CityId = p.CityId
    	INNER JOIN dbo.[State] s ON c.StateId = s.StateId
    	WHERE c.IsDeleted = 0
    	AND s.IsDeleted = 0
    	AND p.IsDeleted = 0
    	AND p.Code = @PostalCode
    	AND s.StateId = @StateId
    	AND c.[Name] = @CityName)
END')");

            migrationBuilder.Sql("ALTER TABLE dbo.Subscriber ADD [PostalGuid] AS ([dbo].[fn_GetPostalGuidForSubscriber]([PostalCode], [City], [StateId]))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
