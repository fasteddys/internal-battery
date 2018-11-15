using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedFKbetweenCourseandVendor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
/*
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodeRedemption_Course_CourseId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropTable(
                name: "CoursePromoCode");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_CourseId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "PromoCodeRedemption");
*/
            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "Course",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
/*
            migrationBuilder.CreateTable(
                name: "CourseVariantPromoCode",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseVariantPromoCodeGuid = table.Column<Guid>(nullable: true),
                    CourseVariantId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariantPromoCode", x => x.CourseVariantPromoCodeId);
                });
*/
            migrationBuilder.CreateIndex(
                name: "IX_Course_VendorId",
                table: "Course",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Vendor_VendorId",
                table: "Course",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "VendorId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Vendor_VendorId",
                table: "Course");
/*
            migrationBuilder.DropTable(
                name: "CourseVariantPromoCode");
*/
            migrationBuilder.DropIndex(
                name: "IX_Course_VendorId",
                table: "Course");
/*
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: 0);
*/
            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "Course",
                nullable: true,
                oldClrType: typeof(int));
/*
            migrationBuilder.CreateTable(
                name: "CoursePromoCode",
                columns: table => new
                {
                    CoursePromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    CoursePromoCodeGuid = table.Column<Guid>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PromoCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePromoCode", x => x.CoursePromoCodeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_CourseId",
                table: "PromoCodeRedemption",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodeRedemption_Course_CourseId",
                table: "PromoCodeRedemption",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
*/
        }
    }
}
