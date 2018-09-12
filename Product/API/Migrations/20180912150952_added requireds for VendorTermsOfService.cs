using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforVendorTermsOfService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorID",
                table: "VendorTermsOfService",
                newName: "VendorId");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorTermsOfServiceGuid",
                table: "VendorTermsOfService",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VendorTermsOfService",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "VendorTermsOfService",
                newName: "VendorID");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorTermsOfServiceGuid",
                table: "VendorTermsOfService",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VendorTermsOfService",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
