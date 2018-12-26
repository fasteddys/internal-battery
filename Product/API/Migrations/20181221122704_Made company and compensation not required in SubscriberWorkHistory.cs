using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class MadecompanyandcompensationnotrequiredinSubscriberWorkHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberWorkHistory_Company_CompanyId",
                table: "SubscriberWorkHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberWorkHistory_CompensationType_CompensationTypeId",
                table: "SubscriberWorkHistory");

            migrationBuilder.AlterColumn<int>(
                name: "CompensationTypeId",
                table: "SubscriberWorkHistory",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "SubscriberWorkHistory",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberWorkHistory_Company_CompanyId",
                table: "SubscriberWorkHistory",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberWorkHistory_CompensationType_CompensationTypeId",
                table: "SubscriberWorkHistory",
                column: "CompensationTypeId",
                principalTable: "CompensationType",
                principalColumn: "CompensationTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberWorkHistory_Company_CompanyId",
                table: "SubscriberWorkHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberWorkHistory_CompensationType_CompensationTypeId",
                table: "SubscriberWorkHistory");

            migrationBuilder.AlterColumn<int>(
                name: "CompensationTypeId",
                table: "SubscriberWorkHistory",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "SubscriberWorkHistory",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberWorkHistory_Company_CompanyId",
                table: "SubscriberWorkHistory",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberWorkHistory_CompensationType_CompensationTypeId",
                table: "SubscriberWorkHistory",
                column: "CompensationTypeId",
                principalTable: "CompensationType",
                principalColumn: "CompensationTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
