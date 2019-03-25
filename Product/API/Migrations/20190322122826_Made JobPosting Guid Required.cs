using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class MadeJobPostingGuidRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "JobPostingGuid",
                table: "JobPosting",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_SecurityClearanceId",
                table: "JobPosting",
                column: "SecurityClearanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_SecurityClearance_SecurityClearanceId",
                table: "JobPosting",
                column: "SecurityClearanceId",
                principalTable: "SecurityClearance",
                principalColumn: "SecurityClearanceId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_SecurityClearance_SecurityClearanceId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_SecurityClearanceId",
                table: "JobPosting");

            migrationBuilder.AlterColumn<Guid>(
                name: "JobPostingGuid",
                table: "JobPosting",
                nullable: true,
                oldClrType: typeof(Guid));
        }
    }
}
