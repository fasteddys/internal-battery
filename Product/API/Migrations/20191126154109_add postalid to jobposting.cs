using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addpostalidtojobposting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostalId",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_PostalId",
                table: "JobPosting",
                column: "PostalId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Postal_PostalId",
                table: "JobPosting",
                column: "PostalId",
                principalTable: "Postal",
                principalColumn: "PostalId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Postal_PostalId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_PostalId",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "PostalId",
                table: "JobPosting");
        }
    }
}
