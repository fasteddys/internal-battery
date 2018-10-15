using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class changedpaymentenrollmentidtoenrollmentguid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentId",
                table: "Payment");

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentGuid",
                table: "Payment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentGuid",
                table: "Payment");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentId",
                table: "Payment",
                nullable: false,
                defaultValue: 0);
        }
    }
}
