using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingServiceOfferingPromoCodeRedemption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOfferingPromoCodeRedemption",
                columns: table => new
                {
                    ServiceOfferingPromoCodeRedemptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ServiceOfferingPromoCodeRedemptionGuid = table.Column<Guid>(nullable: false),
                    RedemptionDate = table.Column<DateTime>(nullable: true),
                    ValueRedeemed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RedemptionNotes = table.Column<string>(nullable: true),
                    RedemptionStatusId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    ServiceOfferingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOfferingPromoCodeRedemption", x => x.ServiceOfferingPromoCodeRedemptionId);
                    table.ForeignKey(
                        name: "FK_ServiceOfferingPromoCodeRedemption_PromoCode_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCode",
                        principalColumn: "PromoCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOfferingPromoCodeRedemption_RedemptionStatus_RedemptionStatusId",
                        column: x => x.RedemptionStatusId,
                        principalTable: "RedemptionStatus",
                        principalColumn: "RedemptionStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOfferingPromoCodeRedemption_ServiceOffering_ServiceOfferingId",
                        column: x => x.ServiceOfferingId,
                        principalTable: "ServiceOffering",
                        principalColumn: "ServiceOfferingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOfferingPromoCodeRedemption_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferingPromoCodeRedemption_PromoCodeId",
                table: "ServiceOfferingPromoCodeRedemption",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferingPromoCodeRedemption_RedemptionStatusId",
                table: "ServiceOfferingPromoCodeRedemption",
                column: "RedemptionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferingPromoCodeRedemption_ServiceOfferingId",
                table: "ServiceOfferingPromoCodeRedemption",
                column: "ServiceOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferingPromoCodeRedemption_SubscriberId",
                table: "ServiceOfferingPromoCodeRedemption",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOfferingPromoCodeRedemption");
        }
    }
}
