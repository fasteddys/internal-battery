using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class campaigncoursevariantonenrollment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Campaign_CampaignId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CampaignId",
                table: "Enrollment");

            migrationBuilder.AddColumn<int>(
                name: "CourseVariantId",
                table: "Enrollment",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contact",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment",
                columns: new[] { "CampaignId", "CourseVariantId" },
                unique: true,
                filter: "[CampaignId] IS NOT NULL AND [CourseVariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Email",
                table: "Contact",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_CampaignCourseVariant_CampaignId_CourseVariantId",
                table: "Enrollment",
                columns: new[] { "CampaignId", "CourseVariantId" },
                principalTable: "CampaignCourseVariant",
                principalColumns: new[] { "CampaignId", "CourseVariantId" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_CampaignCourseVariant_CampaignId_CourseVariantId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Contact_Email",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "CourseVariantId",
                table: "Enrollment");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contact",
                nullable: false,
                oldClrType: typeof(string));
            
            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CampaignId",
                table: "Enrollment",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Campaign_CampaignId",
                table: "Enrollment",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
