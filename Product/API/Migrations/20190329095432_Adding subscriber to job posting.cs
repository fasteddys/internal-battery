using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addingsubscribertojobposting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "JobPosting",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "SubscriberId",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_SubscriberId",
                table: "JobPosting",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Subscriber_SubscriberId",
                table: "JobPosting",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Subscriber_SubscriberId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_SubscriberId",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                table: "JobPosting");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "JobPosting",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Company_CompanyId",
                table: "JobPosting",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
