using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addingpartnertojobapplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "JobApplication",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplication_PartnerId",
                table: "JobApplication",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_Partner_PartnerId",
                table: "JobApplication",
                column: "PartnerId",
                principalTable: "Partner",
                principalColumn: "PartnerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_Partner_PartnerId",
                table: "JobApplication");

            migrationBuilder.DropIndex(
                name: "IX_JobApplication_PartnerId",
                table: "JobApplication");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "JobApplication");
        }
    }
}
