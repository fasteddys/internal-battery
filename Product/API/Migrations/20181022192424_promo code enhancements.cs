using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class promocodeenhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromoTypeName",
                table: "PromoType",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PromoTypeDescription",
                table: "PromoType",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedNumberOfRedemptions",
                table: "PromoCode",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRedemptions",
                table: "PromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "EnrollmentStatus",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "EnrollmentStatus",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "EnrollmentStatus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "EnrollmentStatus",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "EnrollmentStatus",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnrollmentLog",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EnrollmentLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentLogGuid = table.Column<Guid>(nullable: false),
                    EnrollmentTime = table.Column<DateTime>(nullable: false),
                    SubscriberGuid = table.Column<Guid>(nullable: false),
                    CourseGuid = table.Column<Guid>(nullable: false),
                    EnrollmentGuid = table.Column<Guid>(nullable: false),
                    CourseCost = table.Column<decimal>(nullable: false),
                    PromoApplied = table.Column<decimal>(nullable: false),
                    EnrollmentVendorPaymentStatusId = table.Column<int>(nullable: false),
                    EnrollmentVendorInvoicePaymentYear = table.Column<int>(nullable: false),
                    EnrollmentVendorInvoicePaymentMonth = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentLog", x => x.EnrollmentLogId);
                });

            migrationBuilder.CreateTable(
                name: "RedemptionStatus",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    RedemptionStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RedemptionStatusGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedemptionStatus", x => x.RedemptionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "PromoCodeRedemption",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PromoCodeRedemptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCodeRedemptionGuid = table.Column<Guid>(nullable: false),
                    PromoCodeGuid = table.Column<Guid>(nullable: false),
                    StudentGuid = table.Column<Guid>(nullable: false),
                    CourseGuid = table.Column<Guid>(nullable: false),
                    RedemptionDate = table.Column<DateTime>(nullable: false),
                    ValueRedeemed = table.Column<decimal>(nullable: false),
                    RedemptionNotes = table.Column<string>(nullable: true),
                    RedemptionStatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodeRedemption", x => x.PromoCodeRedemptionId);
                    table.ForeignKey(
                        name: "FK_PromoCodeRedemption_RedemptionStatus_RedemptionStatusId",
                        column: x => x.RedemptionStatusId,
                        principalTable: "RedemptionStatus",
                        principalColumn: "RedemptionStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PromoType",
                columns: new[] { "PromoTypeId", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "PromoTypeGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(2018, 10, 22, 15, 24, 23, 298, DateTimeKind.Local), new Guid("7b64effd-ea1d-4fc3-bd5d-006e27e6f0ee"), "This type indicates that the PromoValueFactor is the dollar amount that should be subtracted from the course cost.", 0, null, null, "Dollar Amount", new Guid("aaf54199-9d0b-4f84-b471-1f5d8ff877b6") },
                    { 2, new DateTime(2018, 10, 22, 15, 24, 23, 298, DateTimeKind.Local), new Guid("dc8da61d-d021-484d-8afd-4e16fd6387c6"), "This type indicates that the the course cost should be reduced by the percentage value of the PromoValueFactor.", 0, null, null, "Percent Off", new Guid("c77da13a-141a-48de-be43-d10b095a2e0c") }
                });

            migrationBuilder.InsertData(
                table: "RedemptionStatus",
                columns: new[] { "RedemptionStatusId", "CreateDate", "CreateGuid", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "RedemptionStatusGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(2018, 10, 22, 15, 24, 23, 294, DateTimeKind.Local), new Guid("55d4ffd2-26d3-46f0-9233-999b8dca07b6"), 0, null, null, "In Process", new Guid("f4707d78-c3ae-489f-81ff-51e849126af5") },
                    { 2, new DateTime(2018, 10, 22, 15, 24, 23, 295, DateTimeKind.Local), new Guid("fa5247a4-00d1-4f6b-920f-5f5a001d53f4"), 0, null, null, "Completed", new Guid("5dc22a92-c5dd-4ba3-9553-c055c0f93ea7") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCode_PromoTypeId",
                table: "PromoCode",
                column: "PromoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_RedemptionStatusId",
                table: "PromoCodeRedemption",
                column: "RedemptionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCode_PromoType_PromoTypeId",
                table: "PromoCode",
                column: "PromoTypeId",
                principalTable: "PromoType",
                principalColumn: "PromoTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCode_PromoType_PromoTypeId",
                table: "PromoCode");

            migrationBuilder.DropTable(
                name: "EnrollmentLog");

            migrationBuilder.DropTable(
                name: "PromoCodeRedemption");

            migrationBuilder.DropTable(
                name: "RedemptionStatus");

            migrationBuilder.DropIndex(
                name: "IX_PromoCode_PromoTypeId",
                table: "PromoCode");

            migrationBuilder.DeleteData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PromoType",
                keyColumn: "PromoTypeId",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "MaxAllowedNumberOfRedemptions",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "NumberOfRedemptions",
                table: "PromoCode");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "EnrollmentStatus");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "EnrollmentStatus");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EnrollmentStatus");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "EnrollmentStatus");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "EnrollmentStatus");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PromoType",
                newName: "PromoTypeName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PromoType",
                newName: "PromoTypeDescription");
        }
    }
}
