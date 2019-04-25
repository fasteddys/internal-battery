using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedCompanyGuidtoRecruiterCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "RecruiterCompany",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyGuid",
                table: "RecruiterCompany",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.DropColumn(
                name: "CompanyGuid",
                table: "RecruiterCompany");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "RecruiterCompany",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
