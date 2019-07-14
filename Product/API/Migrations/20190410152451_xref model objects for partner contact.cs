using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class xrefmodelobjectsforpartnercontact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignPartnerContact",
                columns: table => new
                {
                    CampaignId = table.Column<int>(nullable: false),
                    PartnerContactId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CampaignPartnerContactGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPartnerContact", x => new { x.CampaignId, x.PartnerContactId });
                    table.ForeignKey(
                        name: "FK_CampaignPartnerContact_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignPartnerContact_PartnerContact_PartnerContactId",
                        column: x => x.PartnerContactId,
                        principalTable: "PartnerContact",
                        principalColumn: "PartnerContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContactAction",
                columns: table => new
                {
                    PartnerContactId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false),
                    CampaignPhaseId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerContactActionGuid = table.Column<Guid>(nullable: true),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Headers = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactAction", x => new { x.PartnerContactId, x.CampaignId, x.ActionId, x.CampaignPhaseId });
                    table.ForeignKey(
                        name: "FK_PartnerContactAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContactAction_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContactAction_CampaignPhase_CampaignPhaseId",
                        column: x => x.CampaignPhaseId,
                        principalTable: "CampaignPhase",
                        principalColumn: "CampaignPhaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContactAction_PartnerContact_PartnerContactId",
                        column: x => x.PartnerContactId,
                        principalTable: "PartnerContact",
                        principalColumn: "PartnerContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPartnerContact_PartnerContactId",
                table: "CampaignPartnerContact",
                column: "PartnerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactAction_ActionId",
                table: "PartnerContactAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactAction_CampaignId",
                table: "PartnerContactAction",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactAction_CampaignPhaseId",
                table: "PartnerContactAction",
                column: "CampaignPhaseId");

            /* FirstName and LastName no longer exist in the dbo.Contact table - this is preventing migrations from running, so omitting this from future sql migrations
            migrationBuilder.Sql(@"
-- create partner contact records for all contacts that don't already have one (use allegis group)
declare @PartnerId int = (select top 1 PartnerId from Partner where name = 'Allegis Group')
insert into PartnerContact (IsDeleted, CreateDate, CreateGuid, PartnerId, ContactId, PartnerContactGuid, MetaDataJSON)
select 0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', @PartnerId, co.ContactId, NEWID(), 
	(select distinct c.FirstName, c.LastName
	from contact c
	where c.ContactId = co.ContactId
	for json path, without_array_wrapper) [MetaDataJSON]
from contact co
left join PartnerContact pc on co.ContactId = pc.ContactId
where pc.PartnerContactId is null

-- migrate data in CampaignContact to CampaignPartnerContact
insert into CampaignPartnerContact (CampaignId, PartnerContactId, IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, CampaignPartnerContactGuid)
select cc.CampaignId, max(pc.PartnerContactId), cc.IsDeleted, cc.CreateDate, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', NEWID()
from CampaignContact cc
inner join PartnerContact pc on cc.ContactId = pc.ContactId
group by cc.CampaignId, cc.ContactId, cc.IsDeleted, cc.CreateDate

-- migrate data in ContactAction to PartnerContactAction
insert into PartnerContactAction (PartnerContactId, ActionId, CampaignId, CampaignPhaseId, IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, PartnerContactActionGuid, OccurredDate, Headers)
select max(pc.PartnerContactId), ca.ActionId, ca.CampaignId, ca.CampaignPhaseId, ca.IsDeleted, ca.CreateDate, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', NEWID(), ca.OccurredDate, ca.Headers
from ContactAction ca
inner join PartnerContact pc on ca.ContactId = pc.ContactId
group by ca.ActionId, ca.CampaignId, ca.CampaignId, ca.CampaignPhaseId, ca.IsDeleted, ca.CreateDate, ca.OccurredDate, ca.Headers
            ");
            */
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignStatistics] 
AS
BEGIN

 select [CampaignGuid],[name],startDate,endDate,
(select count(*) from CampaignPartnerContact cpc where  cpc.CampaignId = c.CampaignId) as EmailsSent,
(select count(*) from PartnerContactAction pca where ActionId = 1 and pca.CampaignId = c.CampaignId) as OpenEmail,
(select count(*) from PartnerContactAction pca where ActionId = 2 and pca.CampaignId = c.CampaignId) as VisitLandingPage,
(select count(*) from PartnerContactAction pca where ActionId = 3 and pca.CampaignId = c.CampaignId) as CreateAcount,
(select count(*) from PartnerContactAction pca where ActionId = 4 and pca.CampaignId = c.CampaignId) as CourseEnrollment,
(select count(*) from PartnerContactAction pca where ActionId = 5 and pca.CampaignId = c.CampaignId) as CourseCompletion
 from campaign as c order by StartDate desc ;
	 
END
')
            ");

            migrationBuilder.Sql(@"EXEC('

/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.03.12 - Bill Koenig - Modified logic to return campaigns which do not have course offers, exclude logical deletes, ordered output, added example to comment block
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
EXEC [dbo].[System_CampaignDetails] @CampaignGuid = ''AC966B23-B18F-45B1-BE02-07BD475CC6C2''
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
    @CampaignGuid UNIQUEIDENTIFIER 
AS
BEGIN

    SELECT 
		c.Name CourseName, 
		rt.Name RebateType, 
		co.Email, 
		JSON_VALUE(pc.MetaDataJSON, ''$.FirstName'') [FirstName], 
		JSON_VALUE(pc.MetaDataJSON, ''$.LastName'') [LastName], 
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 1 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as OpenEmail,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 2 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as VisitLandingPage,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 3 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CreateAcount,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 4 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseEnrollment,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 5 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseCompletion
    FROM Campaign cp
		INNER JOIN CampaignPartnerContact cpc ON cp.CampaignId = cpc.CampaignId
		INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId
		INNER JOIN Contact co ON pc.ContactId = co.ContactId
		LEFT JOIN CampaignCourseVariant ccv ON ccv.CampaignId = cp.CampaignId
		LEFT JOIN CourseVariant cv ON ccv.CourseVariantId = cv.CourseVariantId
		LEFT JOIN Course c ON cv.CourseId = c.CourseId
		LEFT JOIN RebateType rt ON ccv.RebateTypeId = rt.RebateTypeId
    WHERE cp.CampaignGuid = @CampaignGuid 
		AND co.IsDeleted = 0
		AND cp.IsDeleted = 0
		AND pc.IsDeleted = 0
		AND co.IsDeleted = 0
	ORDER BY OpenEmail DESC, VisitLandingPage DESC, CreateAcount DESC, CourseEnrollment DESC, CourseCompletion DESC

END
')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.02.13=8 - Jim Brazil - Created 
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignStatistics] 
AS
BEGIN

 select [CampaignGuid],[name],startDate,endDate,
(select count(*) from CampaignContact cc where  cc.CampaignId = c.CampaignId) as EmailsSent,
(select count(*) from ContactAction ca where ActionId = 1 and ca.CampaignId = c.CampaignId) as OpenEmail,
(select count(*) from ContactAction ca where ActionId = 2 and ca.CampaignId = c.CampaignId) as VisitLandingPage,
(select count(*) from ContactAction ca where ActionId = 3 and ca.CampaignId = c.CampaignId) as CreateAcount,
(select count(*) from ContactAction ca where ActionId = 4 and ca.CampaignId = c.CampaignId) as CourseEnrollment,
(select count(*) from ContactAction ca where ActionId = 5 and ca.CampaignId = c.CampaignId) as CourseCompletion
 from campaign as c order by StartDate desc ;
	 
END
')
            ");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.03.12 - Bill Koenig - Modified logic to return campaigns which do not have course offers, exclude logical deletes, ordered output, added example to comment block
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
EXEC [dbo].[System_CampaignDetails] @CampaignGuid = ''99C72F03-FF31-4A82-937B-16926EE0D9A2''
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
    @CampaignGuid UNIQUEIDENTIFIER 
AS
BEGIN

    SELECT 
		c.Name CourseName, 
		rt.Name RebateType, 
		co.Email, 
		co.FirstName, 
		co.LastName, 
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 1 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as OpenEmail,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 2 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as VisitLandingPage,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 3 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CreateAcount,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 4 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CourseEnrollment,
		(SELECT COUNT(*) FROM ContactAction ca WHERE ActionId = 5 AND ca.CampaignId = cp.CampaignId AND ca.ContactId = co.ContactId) as CourseCompletion
    FROM Campaign cp
		INNER JOIN CampaignContact cc ON cp.CampaignId = cc.CampaignId
		INNER JOIN Contact co ON cc.ContactId = co.ContactId
		LEFT JOIN CampaignCourseVariant ccv ON ccv.CampaignId = cp.CampaignId
		LEFT JOIN CourseVariant cv ON ccv.CourseVariantId = cv.CourseVariantId
		LEFT JOIN Course c ON cv.CourseId = c.CourseId
		LEFT JOIN RebateType rt ON ccv.RebateTypeId = rt.RebateTypeId
    WHERE cp.CampaignGuid = @CampaignGuid 
		AND co.IsDeleted = 0
		AND cp.IsDeleted = 0
		AND cc.IsDeleted = 0
	ORDER BY OpenEmail DESC, VisitLandingPage DESC, CreateAcount DESC, CourseEnrollment DESC, CourseCompletion DESC

END
')
            ");

            migrationBuilder.DropTable(
                name: "CampaignPartnerContact");

            migrationBuilder.DropTable(
                name: "PartnerContactAction");
        }
    }
}
