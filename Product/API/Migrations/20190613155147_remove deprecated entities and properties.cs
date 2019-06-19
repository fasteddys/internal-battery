using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removedeprecatedentitiesandproperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignContact");

            migrationBuilder.DropTable(
                name: "ContactAction");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Contact");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Contact",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignContact",
                columns: table => new
                {
                    CampaignId = table.Column<int>(nullable: false),
                    ContactId = table.Column<int>(nullable: false),
                    CampaignContactGuid = table.Column<Guid>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    ModifyGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignContact", x => new { x.CampaignId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_CampaignContact_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignContact_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactAction",
                columns: table => new
                {
                    ContactId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    CampaignPhaseId = table.Column<int>(nullable: false),
                    ContactActionGuid = table.Column<Guid>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    Headers = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<int>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactAction", x => new { x.ContactId, x.CampaignId, x.ActionId, x.CampaignPhaseId });
                    table.ForeignKey(
                        name: "FK_ContactAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactAction_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactAction_CampaignPhase_CampaignPhaseId",
                        column: x => x.CampaignPhaseId,
                        principalTable: "CampaignPhase",
                        principalColumn: "CampaignPhaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactAction_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignContact_ContactId",
                table: "CampaignContact",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_ActionId",
                table: "ContactAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_CampaignId",
                table: "ContactAction",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_CampaignPhaseId",
                table: "ContactAction",
                column: "CampaignPhaseId");
        }
    }
}
