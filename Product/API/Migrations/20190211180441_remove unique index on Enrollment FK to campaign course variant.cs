using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removeuniqueindexonEnrollmentFKtocampaigncoursevariant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment",
                columns: new[] { "CampaignId", "CourseVariantId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment");
            
            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CampaignId_CourseVariantId",
                table: "Enrollment",
                columns: new[] { "CampaignId", "CourseVariantId" },
                unique: true,
                filter: "[CampaignId] IS NOT NULL AND [CourseVariantId] IS NOT NULL");
        }
    }
}
