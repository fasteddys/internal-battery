using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addCourseVariantandfixlookupvalues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseVariant",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseGuid = table.Column<Guid>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    VariantType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariant", x => x.CourseVariantId);
                });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), new Guid("1ddb91f6-a6e5-4c01-a020-1dea0ab77e95") });

            migrationBuilder.UpdateData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "PromoTypeGuid" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), new Guid("1ddb91f6-a6e5-4c01-a020-1dea0ab77e95") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 1,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), new Guid("1fe97cde-3a2d-42f1-8b8d-42824367020b") });

            migrationBuilder.UpdateData(
                table: "RedemptionStatus",
                keyColumn: "RedemptionStatusId",
                keyValue: 2,
                columns: new[] { "CreateDate", "CreateGuid", "RedemptionStatusGuid" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), new Guid("1fe97cde-3a2d-42f1-8b8d-42824367020b") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseVariant");

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
        }
    }
}
