using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removeduniqueconstraintonPromoCodeRedemption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId_IsDeleted",
                table: "PromoCodeRedemption");

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 30, 10, 18, 55, 719, DateTimeKind.Local), new Guid("1e44d748-b4a6-4639-8e68-06351765470b"), new Guid("91fd720b-34f2-4b4e-8b4c-34bfc0f9b41e") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 30, 10, 18, 55, 719, DateTimeKind.Local), new Guid("d7422d91-4446-45f3-9212-794fb95ae8a8"), new Guid("719ec7a9-16f4-4542-9ec8-b0fbfdd506be") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 30, 10, 18, 55, 717, DateTimeKind.Local), new Guid("ce5ffd75-5c3c-46a5-9c4b-75de5098754a"), new Guid("0ad0d97f-6c07-48e8-b9cf-4416c5d92579") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 30, 10, 18, 55, 717, DateTimeKind.Local), new Guid("f787bfc9-297a-44ae-911d-4b5c98a49e26"), new Guid("cd7effab-2d28-4ca9-9707-397f807f1a9d") });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId",
                table: "PromoCodeRedemption",
                column: "PromoCodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId",
                table: "PromoCodeRedemption");

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 26, 11, 24, 14, 109, DateTimeKind.Local), new Guid("101b4563-c903-4dc9-8623-b287ef27f877"), new Guid("d5c29d2e-b7a7-460f-81a1-f728915c0dd5") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 26, 11, 24, 14, 110, DateTimeKind.Local), new Guid("2bf5e8a1-be74-45ea-b260-f1499666677e"), new Guid("0a2fa4e9-3fec-4e5c-9cbc-3df321267724") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 26, 11, 24, 14, 107, DateTimeKind.Local), new Guid("05040ee9-60da-4a51-92a5-8c175d1ba8f5"), new Guid("01f55848-de9e-4cee-89f3-eb7f880626ae") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 26, 11, 24, 14, 108, DateTimeKind.Local), new Guid("8b9bd456-bb2f-4be7-b521-d84952378093"), new Guid("4588d5c8-3a2a-4c69-897e-5fe81d147758") });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId_IsDeleted",
                table: "PromoCodeRedemption",
                columns: new[] { "PromoCodeId", "SubscriberId", "CourseId", "RedemptionStatusId", "IsDeleted" },
                unique: true);
        }
    }
}
