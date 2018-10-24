using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class PromoCodeRedemptionenhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentGuid",
                table: "PromoCodeRedemption",
                newName: "SubscriberGuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RedemptionDate",
                table: "PromoCodeRedemption",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromoCodeId",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubscriberId",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: 0);

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
                name: "IX_PromoCodeRedemption_CourseId",
                table: "PromoCodeRedemption",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_SubscriberId",
                table: "PromoCodeRedemption",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId",
                table: "PromoCodeRedemption",
                columns: new[] { "PromoCodeId", "SubscriberId", "CourseId", "RedemptionStatusId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodeRedemption_Course_CourseId",
                table: "PromoCodeRedemption",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodeRedemption_PromoCode_PromoCodeId",
                table: "PromoCodeRedemption",
                column: "PromoCodeId",
                principalTable: "PromoCode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodeRedemption_Subscriber_SubscriberId",
                table: "PromoCodeRedemption",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodeRedemption_Course_CourseId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodeRedemption_PromoCode_PromoCodeId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodeRedemption_Subscriber_SubscriberId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_CourseId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_SubscriberId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId_SubscriberId_CourseId_RedemptionStatusId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                table: "PromoCodeRedemption");

            migrationBuilder.RenameColumn(
                name: "SubscriberGuid",
                table: "PromoCodeRedemption",
                newName: "StudentGuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RedemptionDate",
                table: "PromoCodeRedemption",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 23, 15, 3, 18, 45, DateTimeKind.Local), new Guid("571a2530-2959-43ba-b352-a15915b2b930"), new Guid("c827df73-6796-49f4-84a8-188303f8c060") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 23, 15, 3, 18, 45, DateTimeKind.Local), new Guid("1113bb4e-8c4a-440f-9b33-4d25752ad492"), new Guid("720ab024-73df-42f3-934b-117601739a84") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 23, 15, 3, 18, 42, DateTimeKind.Local), new Guid("d6c208ee-5333-48e5-b928-abda4e1ef421"), new Guid("d87283ed-0409-4e06-b979-5145717d219b") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 23, 15, 3, 18, 43, DateTimeKind.Local), new Guid("d154627f-c823-49f6-8c9f-99fafe2f9187"), new Guid("292c4bda-dd84-4591-a2ab-d8b114cbdc25") });
        }
    }
}
