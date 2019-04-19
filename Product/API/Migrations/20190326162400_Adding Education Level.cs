using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingEducationLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationLevelId",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_EducationLevelId",
                table: "JobPosting",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_ExperienceLevelId",
                table: "JobPosting",
                column: "ExperienceLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_EducationLevel_EducationLevelId",
                table: "JobPosting",
                column: "EducationLevelId",
                principalTable: "EducationLevel",
                principalColumn: "EducationLevelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_ExperienceLevel_ExperienceLevelId",
                table: "JobPosting",
                column: "ExperienceLevelId",
                principalTable: "ExperienceLevel",
                principalColumn: "ExperienceLevelId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_EducationLevel_EducationLevelId",
                table: "JobPosting");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_ExperienceLevel_ExperienceLevelId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_EducationLevelId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_ExperienceLevelId",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "EducationLevelId",
                table: "JobPosting");
        }
    }
}
