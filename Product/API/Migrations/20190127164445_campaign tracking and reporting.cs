using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class campaigntrackingandreporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedNumberOfRedemptions",
                table: "VendorPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRedemptions",
                table: "VendorPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedNumberOfRedemptions",
                table: "SubscriberPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRedemptions",
                table: "SubscriberPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedNumberOfRedemptions",
                table: "CourseVariantPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRedemptions",
                table: "CourseVariantPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Action",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ActionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActionGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Action", x => x.ActionId);
                });

            migrationBuilder.CreateTable(
                name: "Campaign",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CampaignId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CampaignGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaign", x => x.CampaignId);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ContactId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    LinkIdentifier = table.Column<string>(nullable: false),
                    ContactGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_Contact_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberAction",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberActionGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2019, 1, 27, 16, 44, 45, 279, DateTimeKind.Utc))
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
                name: "IX_Contact_SubscriberId",
                table: "Contact",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_ActionId",
                table: "SubscriberAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_CampaignId",
                table: "SubscriberAction",
                column: "CampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "SubscriberAction");

            migrationBuilder.DropTable(
                name: "Action");

            migrationBuilder.DropTable(
                name: "Campaign");

            migrationBuilder.DropColumn(
                name: "MaxAllowedNumberOfRedemptions",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "NumberOfRedemptions",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "MaxAllowedNumberOfRedemptions",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "NumberOfRedemptions",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "MaxAllowedNumberOfRedemptions",
                table: "CourseVariantPromoCode");

            migrationBuilder.DropColumn(
                name: "NumberOfRedemptions",
                table: "CourseVariantPromoCode");
        }
    }
}
