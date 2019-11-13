using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class logicaldeletesseedemails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.06.24 - Bill Koenig - Created
2019.06.24 - Bill Koenig - Modified to take accept a parameter to indicate the scheduled email delivery timestamp
2019.08.06 - Bill Koenig - Corrected order by logic on times used
2019.09.27 - Bill Koenig - Obey logical deletes for Contact and PartnerContact
</remarks>
<description>
Retrieves and updates a single contact record for use as a seed email - this is done to reduce the likelihood that our email sender reputation is damaged.
To cycle through our seed contacts in an evenly distributed fashion, we should always retrieve one which has been used the fewest number of times. In the 
event of a tie for number of times used, select the oldest seed contact (by modified date). 
</description>
<example>
EXEC [dbo].[System_Get_ContactForSeedEmail]
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_ContactForSeedEmail] (
    @DeliveryDate DATETIME = NULL
)
AS
BEGIN

    SET NOCOUNT ON
    		
    DECLARE @PartnerContactId INT
    DECLARE @TimesUsed INT

    SELECT TOP 1 @PartnerContactId = pc.PartnerContactId, @TimesUsed = ISNULL(JSON_VALUE(MetaDataJSON, ''$.TimesUsed''), 0)
    FROM dbo.PartnerContact pc
    INNER JOIN dbo.Contact co ON pc.ContactId = co.ContactId
    INNER JOIN dbo.[Partner] p ON pc.PartnerId = p.PartnerId
    INNER JOIN dbo.PartnerType pt ON p.PartnerTypeId = pt.PartnerTypeId
    WHERE pt.[Name] = ''Email Seed''
	AND pc.IsDeleted = 0
	AND co.IsDeleted = 0
    ORDER BY ISNULL(CONVERT(INT, JSON_VALUE(MetaDataJSON, ''$.TimesUsed'')), 0) ASC, pc.ModifyDate ASC

    UPDATE dbo.PartnerContact
    SET MetaDataJSON = JSON_MODIFY(MetaDataJSON, ''$.TimesUsed'', @TimesUsed + 1)
    	, ModifyDate = ISNULL(@DeliveryDate, GETUTCDATE())
    	, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
    WHERE PartnerContactId = @PartnerContactId

    SELECT pc.*
    FROM dbo.PartnerContact pc
    WHERE pc.PartnerContactId = @PartnerContactId

END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.06.24 - Bill Koenig - Created
2019.06.24 - Bill Koenig - Modified to take accept a parameter to indicate the scheduled email delivery timestamp
2019.08.06 - Bill Koenig - Corrected order by logic on times used
</remarks>
<description>
Retrieves and updates a single contact record for use as a seed email - this is done to reduce the likelihood that our email sender reputation is damaged.
To cycle through our seed contacts in an evenly distributed fashion, we should always retrieve one which has been used the fewest number of times. In the 
event of a tie for number of times used, select the oldest seed contact (by modified date). 
</description>
<example>
EXEC [dbo].[System_Get_ContactForSeedEmail]
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_ContactForSeedEmail] (
    @DeliveryDate DATETIME = NULL
)
AS
BEGIN

    SET NOCOUNT ON
    		
    DECLARE @PartnerContactId INT
    DECLARE @TimesUsed INT

    SELECT TOP 1 @PartnerContactId = pc.PartnerContactId, @TimesUsed = ISNULL(JSON_VALUE(MetaDataJSON, ''$.TimesUsed''), 0)
    FROM dbo.PartnerContact pc
    INNER JOIN dbo.Contact co ON pc.ContactId = co.ContactId
    INNER JOIN dbo.[Partner] p ON pc.PartnerId = p.PartnerId
    INNER JOIN dbo.PartnerType pt ON p.PartnerTypeId = pt.PartnerTypeId
    WHERE pt.[Name] = ''Email Seed''
    ORDER BY ISNULL(CONVERT(INT, JSON_VALUE(MetaDataJSON, ''$.TimesUsed'')), 0) ASC, pc.ModifyDate ASC

    UPDATE dbo.PartnerContact
    SET MetaDataJSON = JSON_MODIFY(MetaDataJSON, ''$.TimesUsed'', @TimesUsed + 1)
    	, ModifyDate = ISNULL(@DeliveryDate, GETUTCDATE())
    	, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
    WHERE PartnerContactId = @PartnerContactId

    SELECT pc.*
    FROM dbo.PartnerContact pc
    WHERE pc.PartnerContactId = @PartnerContactId

END
            ')");
        }
    }
}
