using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class renamerebatetorefundaddrebatetypeaddcampaigncoursevariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rebate");

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "Enrollment",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RebateType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    RebateTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RebateTypeGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebateType", x => x.RebateTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Refund",
                columns: table => new
                {
                    RefundId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RefundGuid = table.Column<Guid>(nullable: true),
                    EnrollmentId = table.Column<int>(nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundIssued = table.Column<int>(nullable: false),
                    RefundIssueDate = table.Column<DateTime>(nullable: false),
                    RefundIssueStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refund", x => x.RefundId);
                });

            migrationBuilder.CreateTable(
                name: "CampaignCourseVariant",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CampaignId = table.Column<int>(nullable: false),
                    CourseVariantId = table.Column<int>(nullable: false),
                    CampaignCourseVariantGuid = table.Column<Guid>(nullable: true),
                    MaxRebateEligibilityInDays = table.Column<int>(nullable: true),
                    IsEligibleForRebate = table.Column<bool>(nullable: false),
                    RebateTypeId = table.Column<int>(nullable: false),
                    RefundId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCourseVariant", x => new { x.CampaignId, x.CourseVariantId });
                    table.ForeignKey(
                        name: "FK_CampaignCourseVariant_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCourseVariant_CourseVariant_CourseVariantId",
                        column: x => x.CourseVariantId,
                        principalTable: "CourseVariant",
                        principalColumn: "CourseVariantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCourseVariant_RebateType_RebateTypeId",
                        column: x => x.RebateTypeId,
                        principalTable: "RebateType",
                        principalColumn: "RebateTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCourseVariant_Refund_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refund",
                        principalColumn: "RefundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 2,
                column: "Name",
                value: "Visit landing page");

            migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "ActionId", "ActionGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[] { 5, new Guid("ccbee3a5-278e-4696-9cb6-ab6dc5b50d0a"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Complete course" });

            migrationBuilder.InsertData(
                table: "RebateType",
                columns: new[] { "RebateTypeId", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "RebateTypeGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(2019, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "Get hired by one of our staffing partners for a full refund!", 0, null, null, "Employment", new Guid("b7be76f3-ac8d-4c64-93f3-a62cc09d8dde") },
                    { 2, new DateTime(2019, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "Complete the course within the offer time period for a full refund!", 0, null, null, "Course completion", new Guid("fb69d56d-686b-4969-9465-e994e6c599a1") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CampaignId",
                table: "Enrollment",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCourseVariant_CourseVariantId",
                table: "CampaignCourseVariant",
                column: "CourseVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCourseVariant_RebateTypeId",
                table: "CampaignCourseVariant",
                column: "RebateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCourseVariant_RefundId",
                table: "CampaignCourseVariant",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_RebateType_Name",
                table: "RebateType",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Campaign_CampaignId",
                table: "Enrollment",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Campaign_CampaignId",
                table: "Enrollment");

            migrationBuilder.DropTable(
                name: "CampaignCourseVariant");

            migrationBuilder.DropTable(
                name: "RebateType");

            migrationBuilder.DropTable(
                name: "Refund");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CampaignId",
                table: "Enrollment");

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Enrollment");

            migrationBuilder.CreateTable(
                name: "Rebate",
                columns: table => new
                {
                    RebateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentId = table.Column<int>(nullable: false),
                    RebateAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RebateGuid = table.Column<Guid>(nullable: true),
                    RebateIssueDate = table.Column<DateTime>(nullable: false),
                    RebateIssueStatus = table.Column<int>(nullable: false),
                    RebateIssued = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebate", x => x.RebateId);
                });

            migrationBuilder.UpdateData(
                table: "Action",
                keyColumn: "ActionId",
                keyValue: 2,
                column: "Name",
                value: "View content");
        }
    }
}
