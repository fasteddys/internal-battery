using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addedcityandstatereferencestosubscribertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "Subscriber",
                nullable: false,
                defaultValue: 0);
/*
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
 */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "Subscriber");
            /*
            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 25, 12, 7, 30, 224, DateTimeKind.Local), new Guid("e1cd0af7-5bd6-40ca-9a48-92e568ce8fc3"), new Guid("ee99d1b8-82b7-4c46-84ee-481f97654ce4") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 25, 12, 7, 30, 224, DateTimeKind.Local), new Guid("a7f16a7c-cbc0-4890-84c2-97728b0ec90c"), new Guid("02342c90-4407-4f2f-96f2-5688956dffa0") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 25, 12, 7, 30, 222, DateTimeKind.Local), new Guid("2c32f6d8-39a0-4512-947a-774bee252e4f"), new Guid("e87cd4cd-5234-4f8f-bcc7-25de2756bd1b") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 25, 12, 7, 30, 222, DateTimeKind.Local), new Guid("0339ef2c-219d-47c4-97d5-fa07b9f6b076"), new Guid("d19ee69f-48f9-45b1-8e0c-8775943ea42d") });
        */
        }
    }
}
