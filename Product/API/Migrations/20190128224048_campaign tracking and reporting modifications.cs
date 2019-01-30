using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class campaigntrackingandreportingmodifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberAction");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Campaign",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Action",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactAction",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ContactActionGuid = table.Column<Guid>(nullable: true),
                    ContactId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactAction", x => new { x.ContactId, x.CampaignId, x.ActionId });
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
                        name: "FK_ContactAction_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "ActionId", "ActionGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    { 1, new Guid("8653122b-74f1-4020-8812-04c355ce56e7"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Open email" },
                    { 2, new Guid("47d62280-213f-44f3-8085-a83bb2a5bbe3"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "View content" },
                    { 3, new Guid("5d87152d-b0d3-4a94-a777-58b69a44950e"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Create account" },
                    { 4, new Guid("3fa0b888-acc6-4da9-8e86-03f59f3137f5"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Course enrollment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_ActionId",
                table: "ContactAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_CampaignId",
                table: "ContactAction",
                column: "CampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactAction");

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Action");
            
            migrationBuilder.CreateTable(
                name: "SubscriberAction",
                columns: table => new
                {
                    SubscriberId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2019, 1, 27, 16, 44, 45, 279, DateTimeKind.Utc)),
                    SubscriberActionGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberAction", x => new { x.SubscriberId, x.CampaignId, x.ActionId });
                    table.ForeignKey(
                        name: "FK_SubscriberAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberAction_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberAction_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_ActionId",
                table: "SubscriberAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_CampaignId",
                table: "SubscriberAction",
                column: "CampaignId");
        }
    }
}
