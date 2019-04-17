using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Madenavigationpropertievirtual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_CompensationTypeId",
                table: "JobPosting",
                column: "CompensationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_CompensationType_CompensationTypeId",
                table: "JobPosting",
                column: "CompensationTypeId",
                principalTable: "CompensationType",
                principalColumn: "CompensationTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_CompensationType_CompensationTypeId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_CompensationTypeId",
                table: "JobPosting");
        }
    }
}
