using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removeunnecessarycolumnsfromPromoCodeRedemptionupdateuniqueindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "CourseGuid",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "PromoCodeGuid",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "SubscriberGuid",
                table: "PromoCodeRedemption");

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

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId_IsDeleted",
                table: "PromoCodeRedemption",
                columns: new[] { "PromoCodeId", "SubscriberId", "CourseId", "RedemptionStatusId", "IsDeleted" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId_IsDeleted",
                table: "PromoCodeRedemption");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseGuid",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PromoCodeGuid",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberGuid",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 24, 17, 34, 53, 583, DateTimeKind.Local), new Guid("dc2eade1-2fcb-468c-98f3-affa98d6d6a9"), new Guid("85e55590-c1e0-42a2-987e-7c1f8ddee780") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 24, 17, 34, 53, 584, DateTimeKind.Local), new Guid("13d81c09-6b2d-4dad-8473-b0d735fd383d"), new Guid("2fcddbf7-ab44-42d9-8953-059c567e23a2") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 24, 17, 34, 53, 581, DateTimeKind.Local), new Guid("5008b2c2-6ab9-4c9c-9533-d431c4b339fe"), new Guid("8cf95a48-293f-4c42-9933-63938bc41af3") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 24, 17, 34, 53, 582, DateTimeKind.Local), new Guid("495a2404-f3f3-4328-8dd1-81d797cf4e70"), new Guid("29167203-f040-4c21-9c06-5b228f550818") });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId",
                table: "PromoCodeRedemption",
                columns: new[] { "PromoCodeId", "SubscriberId", "CourseId", "RedemptionStatusId" },
                unique: true);
        }
    }
}
