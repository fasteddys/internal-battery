using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class countryandstatecodesaddbasemodelforpromorestrictionsSectionStartTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "VendorPromoCode",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "VendorPromoCode",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "VendorPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "VendorPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "VendorPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SubscriberPromoCode",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "SubscriberPromoCode",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "SubscriberPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "SubscriberPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "SubscriberPromoCode",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SectionStartTimestamp",
                table: "Enrollment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "CoursePromoCode",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "CoursePromoCode",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "CoursePromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "CoursePromoCode",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "CoursePromoCode",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CountryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CountryGuid = table.Column<Guid>(nullable: true),
                    Code2 = table.Column<string>(nullable: false),
                    Code3 = table.Column<string>(nullable: false),
                    OfficialName = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    StateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StateGuid = table.Column<Guid>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CountryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateId);
                    table.ForeignKey(
                        name: "FK_State_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_State_CountryId",
                table: "State",
                column: "CountryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "SectionStartTimestamp",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "CoursePromoCode");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "CoursePromoCode");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CoursePromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "CoursePromoCode");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "CoursePromoCode");

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 22, 15, 24, 23, 298, DateTimeKind.Local), new Guid("7b64effd-ea1d-4fc3-bd5d-006e27e6f0ee"), new Guid("aaf54199-9d0b-4f84-b471-1f5d8ff877b6") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(2018, 10, 22, 15, 24, 23, 298, DateTimeKind.Local), new Guid("dc8da61d-d021-484d-8afd-4e16fd6387c6"), new Guid("c77da13a-141a-48de-be43-d10b095a2e0c") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 22, 15, 24, 23, 294, DateTimeKind.Local), new Guid("55d4ffd2-26d3-46f0-9233-999b8dca07b6"), new Guid("f4707d78-c3ae-489f-81ff-51e849126af5") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(2018, 10, 22, 15, 24, 23, 295, DateTimeKind.Local), new Guid("fa5247a4-00d1-4f6b-920f-5f5a001d53f4"), new Guid("5dc22a92-c5dd-4ba3-9553-c055c0f93ea7") });
        }
    }
}
