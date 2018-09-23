using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedEnrollmentGuidtoWozTransactionLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentId",
                table: "WozTransactionLog");

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentGuid",
                table: "WozTransactionLog",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentGuid",
                table: "WozTransactionLog");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentId",
                table: "WozTransactionLog",
                nullable: false,
                defaultValue: 0);
        }
    }
}
