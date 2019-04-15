using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class changedtablenames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobApplication_JobPosting_JobPostingId",
                table: "jobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_jobApplication_Subscriber_SubscriberId",
                table: "jobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_recruiterCompany_Company_CompanyId",
                table: "recruiterCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_recruiterCompany_Subscriber_SubscriberId",
                table: "recruiterCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_recruiterCompany",
                table: "recruiterCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_jobApplication",
                table: "jobApplication");

            migrationBuilder.RenameTable(
                name: "recruiterCompany",
                newName: "RecruiterCompany");

            migrationBuilder.RenameTable(
                name: "jobApplication",
                newName: "JobApplication");

            migrationBuilder.RenameIndex(
                name: "IX_recruiterCompany_SubscriberId",
                table: "RecruiterCompany",
                newName: "IX_RecruiterCompany_SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_recruiterCompany_CompanyId",
                table: "RecruiterCompany",
                newName: "IX_RecruiterCompany_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_jobApplication_SubscriberId",
                table: "JobApplication",
                newName: "IX_JobApplication_SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_jobApplication_JobPostingId",
                table: "JobApplication",
                newName: "IX_JobApplication_JobPostingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecruiterCompany",
                table: "RecruiterCompany",
                column: "RecruiterCompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobApplication",
                table: "JobApplication",
                column: "JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_JobPosting_JobPostingId",
                table: "JobApplication",
                column: "JobPostingId",
                principalTable: "JobPosting",
                principalColumn: "JobPostingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_Subscriber_SubscriberId",
                table: "JobApplication",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Subscriber_SubscriberId",
                table: "RecruiterCompany",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_JobPosting_JobPostingId",
                table: "JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_Subscriber_SubscriberId",
                table: "JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Subscriber_SubscriberId",
                table: "RecruiterCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecruiterCompany",
                table: "RecruiterCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobApplication",
                table: "JobApplication");

            migrationBuilder.RenameTable(
                name: "RecruiterCompany",
                newName: "recruiterCompany");

            migrationBuilder.RenameTable(
                name: "JobApplication",
                newName: "jobApplication");

            migrationBuilder.RenameIndex(
                name: "IX_RecruiterCompany_SubscriberId",
                table: "recruiterCompany",
                newName: "IX_recruiterCompany_SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_RecruiterCompany_CompanyId",
                table: "recruiterCompany",
                newName: "IX_recruiterCompany_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_SubscriberId",
                table: "jobApplication",
                newName: "IX_jobApplication_SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_JobPostingId",
                table: "jobApplication",
                newName: "IX_jobApplication_JobPostingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_recruiterCompany",
                table: "recruiterCompany",
                column: "RecruiterCompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_jobApplication",
                table: "jobApplication",
                column: "JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_jobApplication_JobPosting_JobPostingId",
                table: "jobApplication",
                column: "JobPostingId",
                principalTable: "JobPosting",
                principalColumn: "JobPostingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_jobApplication_Subscriber_SubscriberId",
                table: "jobApplication",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_recruiterCompany_Company_CompanyId",
                table: "recruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_recruiterCompany_Subscriber_SubscriberId",
                table: "recruiterCompany",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
