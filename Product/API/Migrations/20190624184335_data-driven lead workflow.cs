using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class datadrivenleadworkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailDeliveryDate",
                table: "CampaignPartnerContact",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailSent",
                table: "CampaignPartnerContact",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignPartner",
                columns: table => new
                {
                    CampaignPartnerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CampaignId = table.Column<int>(nullable: false),
                    PartnerId = table.Column<int>(nullable: false),
                    CampaignPartnerGuid = table.Column<Guid>(nullable: true),
                    EmailDeliveryCap = table.Column<int>(nullable: true),
                    EmailDeliveryLookbackInHours = table.Column<int>(nullable: true),
                    IsUseSeedEmails = table.Column<bool>(nullable: false, defaultValueSql: "0"),
                    EmailTemplateId = table.Column<string>(maxLength: 50, nullable: false),
                    EmailSubAccountId = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPartner", x => x.CampaignPartnerId);
                    table.ForeignKey(
                        name: "FK_CampaignPartner_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPartner_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPartner_CampaignId",
                table: "CampaignPartner",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPartner_PartnerId",
                table: "CampaignPartner",
                column: "PartnerId");

            migrationBuilder.InsertData(
                table: "PartnerType",
                columns: new[] { "PartnerTypeGuid", "Name", "Description", "IsDeleted", "CreateDate", "CreateGuid" },
                values: new object[,]
                {
                    { new Guid("78118EA2-A81D-4B2F-86DC-578CF0B2FFDA"), "Email Seed", "Contacts associated with a Partner whose type is 'Email Seed' are eligible to be sent to help preserve our email sender reputation.", 0, new DateTime(2019, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.Sql(@"
INSERT INTO dbo.Partner (IsDeleted, CreateDate, CreateGuid, PartnerGuid, Name, Description, PartnerTypeId) 
VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', 'B61E7451-3AAE-4513-8D79-C4B0EAC7F93B', 'Internal Allegis Employees', 'These email addresses were sourced from Allegis Garage employees as well as other people known to Brittany Bramhall', (SELECT TOP 1 PartnerTypeId FROM dbo.PartnerType WHERE [Name] = 'Email Seed'))");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.06.24 - Bill Koenig - Created
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
CREATE PROCEDURE [dbo].[System_Get_ContactForSeedEmail]
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
		, ModifyDate = GETUTCDATE()
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
            migrationBuilder.DropTable(
                name: "CampaignPartner");

            migrationBuilder.DropColumn(
                name: "EmailDeliveryDate",
                table: "CampaignPartnerContact");

            migrationBuilder.DropColumn(
                name: "IsEmailSent",
                table: "CampaignPartnerContact");
            
            migrationBuilder.DeleteData("Partner", "Name", "Internal Allegis Employees");

            migrationBuilder.DeleteData("PartnerType", "Name", "Email Seed");

            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_Get_ContactForSeedEmail]");
        }
    }
}
