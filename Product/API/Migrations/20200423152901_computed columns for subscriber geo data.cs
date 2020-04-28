using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class computedcolumnsforsubscribergeodata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-04-22 - Bill Koenig - Created
</remarks>
<description>
Returns a CityGuid that aligns with the city and state for the subscriber. If a state has not been selected, no value will be returned.
</description>
<example>
SELECT [dbo].[fn_GetCityGuidForSubscriber](''Bel Air'', NULL) -- invalid
SELECT [dbo].[fn_GetCityGuidForSubscriber](''Bel Air'', 1570) -- valid 
</example>
*/
CREATE FUNCTION [dbo].[fn_GetCityGuidForSubscriber](@CityName NVARCHAR(MAX), @StateId INT)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN

RETURN (SELECT TOP 1 c.CityGuid
		FROM dbo.City c
		INNER JOIN dbo.[State] s ON c.StateId = s.StateId
		WHERE c.IsDeleted = 0
		AND s.IsDeleted = 0
		AND s.StateId = @StateId
		AND c.[Name] = @CityName)
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-04-22 - Bill Koenig - Created
</remarks>
<description>
Returns a StateGuid that aligns with the state for the subscriber. If a state has not been selected, no value will be returned.
</description>
<example>
SELECT [dbo].[fn_GetStateGuidForSubscriber](NULL) -- invalid
SELECT [dbo].[fn_GetStateGuidForSubscriber](1570) -- valid 
</example>
*/
CREATE FUNCTION [dbo].[fn_GetStateGuidForSubscriber](@StateId INT)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN

RETURN (SELECT TOP 1 s.StateGuid
		FROM dbo.[State] s 
		WHERE s.IsDeleted = 0
		AND s.StateId = @StateId)
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-04-22 - Bill Koenig - Created
</remarks>
<description>
Returns a PostalGuid that aligns with the city, state, and postal code for the subscriber. If a valid city, state, and postal combination has not been selected, no value will be returned.
</description>
<example>
SELECT [dbo].[fn_GetPostalGuidForSubscriber](''90210'', ''Bel Air'', NULL) -- invalid
SELECT [dbo].[fn_GetPostalGuidForSubscriber](''21015'', ''Bel Air'', 1570) -- valid 
</example>
*/
CREATE FUNCTION [dbo].[fn_GetPostalGuidForSubscriber](@PostalCode NVARCHAR(MAX), @CityName NVARCHAR(MAX), @StateId INT)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN

RETURN (SELECT TOP 1 c.CityGuid
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

            migrationBuilder.AddColumn<Guid>(
                name: "CityGuid",
                table: "Subscriber",
                nullable: true,
                computedColumnSql: "([dbo].[fn_GetCityGuidForSubscriber]([City], [StateId]))");

            migrationBuilder.AddColumn<Guid>(
                name: "PostalGuid",
                table: "Subscriber",
                nullable: true,
                computedColumnSql: "([dbo].[fn_GetPostalGuidForSubscriber]([PostalCode], [City], [StateId]))");

            migrationBuilder.AddColumn<Guid>(
                name: "StateGuid",
                table: "Subscriber",
                nullable: true,
                computedColumnSql: "([dbo].[fn_GetStateGuidForSubscriber]([StateId]))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityGuid",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "PostalGuid",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "StateGuid",
                table: "Subscriber");

            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_GetPostalGuidForSubscriber]");

            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_GetStateGuidForSubscriber]");

            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_GetCityGuidForSubscriber]");
        }
    }
}
