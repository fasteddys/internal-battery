using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class unsubscribegroupfortransactionalemail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnsubscribeGroupId",
                table: "CampaignPartner",
                nullable: true);

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-06-26 - Bill Koenig - Created
2019-07-18 - Bill Koenig - Including unsubscribe group id for transactional emails
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
		INNER JOIN CampaignPartner cp ON ca.CampaignId = cp.CampaignId)
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

            migrationBuilder.Sql("UPDATE dbo.CampaignPartner SET UnsubscribeGroupId = 17302");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnsubscribeGroupId",
                table: "CampaignPartner");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-06-26 - Bill Koenig - Created
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
			, ISNULL(CASE WHEN cp.EmailDeliveryCap IS NOT NULL AND cp.EmailDeliveryLookbackInHours IS NOT NULL THEN cp.EmailDeliveryCap / cp.EmailDeliveryLookbackInHours ELSE NULL END, 2147483647) [HourlyDeliveryLimit]
		FROM Campaign ca
		INNER JOIN CampaignPartner cp ON ca.CampaignId = cp.CampaignId)
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
        }
    }
}
