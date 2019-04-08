using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class makedupecheckparamsnullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.04.01 - Bill Koenig - Created
2019.04.03 - Bill Koenig - Made parameters nullable, optimized to seek on indexes
</remarks>
<description>
Performs a contact dupe check search on email or phone with an infinite lookback window
</description>
<example>
DECLARE @IsDupeOutput BIT
EXEC [dbo].[System_Get_LeadDupeCheck] 
	@Email = ''delorean6971@gmail.com'', 
	@Phone = ''4105151072'', 
	@IsDupe = @IsDupeOutput OUTPUT
SELECT @IsDupeOutput [IsDupe]
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_LeadDupeCheck] (
	@Email NVARCHAR(100) = NULL,
	@Phone NVARCHAR(10) = NULL,
	@IsDupe BIT OUTPUT
)
AS
BEGIN

	SET @IsDupe = 0;

	IF EXISTS(
		SELECT c.ContactGuid
		FROM Contact c WITH(NOLOCK)
		WHERE c.Email = @Email) 
		OR EXISTS(
		SELECT c.ContactGuid
		FROM Contact c WITH(NOLOCK)
		INNER JOIN PartnerContact pc WITH(NOLOCK) ON c.ContactId = pc.ContactId
		WHERE pc.vMobilePhone = @Phone 
		)
	BEGIN
		SET @IsDupe = 1;
	END
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE dbo.System_Get_LeadDupeCheck");
        }
    }
}
