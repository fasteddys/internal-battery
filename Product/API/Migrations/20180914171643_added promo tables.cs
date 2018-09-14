using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedpromotables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnrollmentStatusId",
                table: "Enrollment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoursePromoCode",
                columns: table => new
                {
                    CoursePromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CoursePromoCodeGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePromoCode", x => x.CoursePromoCodeId);
                });

            migrationBuilder.CreateTable(
                name: "PromoCode",
                columns: table => new
                {
                    PromoCodesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCodesGuid = table.Column<Guid>(nullable: true),
                    PromoCode = table.Column<string>(nullable: true),
                    PromoStartDate = table.Column<DateTime>(nullable: false),
                    PromoEndDate = table.Column<DateTime>(nullable: false),
                    PromoTypeId = table.Column<int>(nullable: false),
                    PromoValueFacotr = table.Column<decimal>(nullable: false),
                    PromoName = table.Column<string>(nullable: true),
                    PromoDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCode", x => x.PromoCodesId);
                });

            migrationBuilder.CreateTable(
                name: "PromoType",
                columns: table => new
                {
                    PromoTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoTypeGuid = table.Column<Guid>(nullable: true),
                    PromoTypeName = table.Column<string>(nullable: true),
                    PromoTypeDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoType", x => x.PromoTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Rebate",
                columns: table => new
                {
                    RebateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RebateGuid = table.Column<Guid>(nullable: true),
                    EnrollmentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebate", x => x.RebateId);
                });

            migrationBuilder.CreateTable(
                name: "ReportEnrollmentByVendors",
                columns: table => new
                {
                    ReportEnrollmentByVendorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ReportEnrollmentByVendorGuid = table.Column<Guid>(nullable: true),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    EnrollmentsCompleted = table.Column<int>(nullable: false),
                    VendorId = table.Column<int>(nullable: false),
                    TotalRevenueIn = table.Column<decimal>(nullable: false),
                    TotalSplitOut = table.Column<decimal>(nullable: false),
                    CoursesCompletedCount = table.Column<int>(nullable: false),
                    CoursesDroppedCount = table.Column<int>(nullable: false),
                    TotalRefundsIssued = table.Column<decimal>(nullable: false),
                    PromoCodesRedeemedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportEnrollmentByVendors", x => x.ReportEnrollmentByVendorId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberPromoCode",
                columns: table => new
                {
                    SubscriberPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberPromoCodeGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberPromoCode", x => x.SubscriberPromoCodeId);
                });

            migrationBuilder.CreateTable(
                name: "VendorPromoCode",
                columns: table => new
                {
                    VendorPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VendorPromoCodeGuid = table.Column<Guid>(nullable: true),
                    PromoCodeId = table.Column<int>(nullable: false),
                    VendorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorPromoCode", x => x.VendorPromoCodeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoursePromoCode");

            migrationBuilder.DropTable(
                name: "PromoCode");

            migrationBuilder.DropTable(
                name: "PromoType");

            migrationBuilder.DropTable(
                name: "Rebate");

            migrationBuilder.DropTable(
                name: "ReportEnrollmentByVendors");

            migrationBuilder.DropTable(
                name: "SubscriberPromoCode");

            migrationBuilder.DropTable(
                name: "VendorPromoCode");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatusId",
                table: "Enrollment");
        }
    }
}
