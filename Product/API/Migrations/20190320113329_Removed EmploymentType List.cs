using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class RemovedEmploymentTypeList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentType_JobPosting_JobPostingId",
                table: "EmploymentType");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentType_JobPostingId",
                table: "EmploymentType");

            migrationBuilder.DropColumn(
                name: "JobPostingId",
                table: "EmploymentType");

            migrationBuilder.AddColumn<int>(
                name: "EmploymentTypeId",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_CompanyId",
                table: "JobPosting",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_EmploymentTypeId",
                table: "JobPosting",
                column: "EmploymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_EmploymentType_EmploymentTypeId",
                table: "JobPosting",
                column: "EmploymentTypeId",
                principalTable: "EmploymentType",
                principalColumn: "EmploymentTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_EmploymentType_EmploymentTypeId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_CompanyId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_EmploymentTypeId",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeId",
                table: "JobPosting");

            migrationBuilder.AddColumn<int>(
                name: "JobPostingId",
                table: "EmploymentType",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentType_JobPostingId",
                table: "EmploymentType",
                column: "JobPostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentType_JobPosting_JobPostingId",
                table: "EmploymentType",
                column: "JobPostingId",
                principalTable: "JobPosting",
                principalColumn: "JobPostingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
