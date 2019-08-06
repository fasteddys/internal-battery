using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class emailsuppressionandseedemailfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-06-26 - Bill Koenig - Created
2019-07-18 - Bill Koenig - Including unsubscribe group id for transactional emails and ignoring campaign partners which have been logically deleted
2019-08-06 - Bill Koenig - Added duplicate check to email suppression logic
</remarks>
<description>
Returns a list of the lead emails which may be sent for current delivery window
</description>
<example>
SELECT * FROM [dbo].[v_ThrottledLeadEmailDelivery]
</example>
*/
ALTER VIEW [dbo].[v_ThrottledLeadEmailDelivery] 
AS 

	WITH campaignCaps AS (
		SELECT ca.CampaignId
			, cp.IsUseSeedEmails
			, cp.EmailSubAccountId
			, cp.EmailTemplateId
			, cp.UnsubscribeGroupId
			, ISNULL(CASE WHEN cp.EmailDeliveryCap IS NOT NULL AND cp.EmailDeliveryLookbackInHours IS NOT NULL THEN cp.EmailDeliveryCap / cp.EmailDeliveryLookbackInHours ELSE NULL END, 2147483647) [HourlyDeliveryLimit]
		FROM Campaign ca
		INNER JOIN CampaignPartner cp ON ca.CampaignId = cp.CampaignId
		WHERE cp.IsDeleted = 0)
	, emailSuppression AS (
		SELECT pcls.PartnerContactId
		FROM PartnerContactLeadStatus pcls 
		INNER JOIN LeadStatus ls ON pcls.LeadStatusId = ls.LeadStatusId
		WHERE ls.[Name] IN (''Verification Failure'', ''Duplicate'')
	)
	SELECT EmailsToDeliverThisHour.*
	FROM campaignCaps cc
	CROSS APPLY (
		SELECT TOP (cc.HourlyDeliveryLimit)	cpc.PartnerContactId
			, ca.CampaignId
			, co.Email
			, JSON_VALUE(pc.MetaDataJSON, ''$.FirstName'') [FirstName]
			, JSON_VALUE(pc.MetaDataJSON, ''$.LastName'') [LastName]
			, cpc.TinyId		
			, cc.IsUseSeedEmails
			, cc.EmailSubAccountId
			, cc.EmailTemplateId
			, cc.UnsubscribeGroupId
		FROM dbo.CampaignPartnerContact cpc
		INNER JOIN Campaign ca ON cpc.CampaignId = ca.CampaignId
		INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId
		INNER JOIN Contact co ON pc.ContactId = co.ContactId
		LEFT JOIN emailSuppression es ON pc.PartnerContactId = es.PartnerContactId
		WHERE cpc.IsEmailSent = 0
		AND cc.CampaignId = ca.CampaignId
		AND es.PartnerContactId IS NULL
		ORDER BY cpc.CreateDate DESC) AS EmailsToDeliverThisHour
            ')");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-06-26 - Bill Koenig - Created
2019-07-18 - Bill Koenig - Including unsubscribe group id for transactional emails and ignoring campaign partners which have been logically deleted
</remarks>
<description>
Returns a list of the lead emails which may be sent for current delivery window
</description>
<example>
SELECT * FROM [dbo].[v_ThrottledLeadEmailDelivery]
</example>
*/
ALTER VIEW [dbo].[v_ThrottledLeadEmailDelivery] 
AS 

	WITH campaignCaps AS (
		SELECT ca.CampaignId
			, cp.IsUseSeedEmails
			, cp.EmailSubAccountId
			, cp.EmailTemplateId
			, cp.UnsubscribeGroupId
			, ISNULL(CASE WHEN cp.EmailDeliveryCap IS NOT NULL AND cp.EmailDeliveryLookbackInHours IS NOT NULL THEN cp.EmailDeliveryCap / cp.EmailDeliveryLookbackInHours ELSE NULL END, 2147483647) [HourlyDeliveryLimit]
		FROM Campaign ca
		INNER JOIN CampaignPartner cp ON ca.CampaignId = cp.CampaignId
		WHERE cp.IsDeleted = 0)
	, verificationFailures AS (
		SELECT pcls.PartnerContactId
		FROM PartnerContactLeadStatus pcls 
		INNER JOIN LeadStatus ls ON pcls.LeadStatusId = ls.LeadStatusId
		WHERE ls.[Name] = ''Verification Failure''
	)
	SELECT EmailsToDeliverThisHour.*
	FROM campaignCaps cc
	CROSS APPLY (
		SELECT TOP (cc.HourlyDeliveryLimit)	cpc.PartnerContactId
			, ca.CampaignId
			, co.Email
			, JSON_VALUE(pc.MetaDataJSON, ''$.FirstName'') [FirstName]
			, JSON_VALUE(pc.MetaDataJSON, ''$.LastName'') [LastName]
			, cpc.TinyId		
			, cc.IsUseSeedEmails
			, cc.EmailSubAccountId
			, cc.EmailTemplateId
			, cc.UnsubscribeGroupId
		FROM dbo.CampaignPartnerContact cpc
		INNER JOIN Campaign ca ON cpc.CampaignId = ca.CampaignId
		INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId
		INNER JOIN Contact co ON pc.ContactId = co.ContactId
		LEFT JOIN verificationFailures vf ON pc.PartnerContactId = vf.PartnerContactId
		WHERE cpc.IsEmailSent = 0
		AND cc.CampaignId = ca.CampaignId
		AND vf.PartnerContactId IS NULL
		ORDER BY cpc.CreateDate DESC) AS EmailsToDeliverThisHour
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.06.24 - Bill Koenig - Created
2019.06.24 - Bill Koenig - Modified to take accept a parameter to indicate the scheduled email delivery timestamp
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
	ORDER BY ISNULL(JSON_VALUE(MetaDataJSON, ''$.TimesUsed''), 0) ASC, pc.ModifyDate ASC

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
