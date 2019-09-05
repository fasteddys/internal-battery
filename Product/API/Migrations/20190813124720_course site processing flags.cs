using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class coursesiteprocessingflags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCrawling",
                table: "CourseSite",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSyncing",
                table: "CourseSite",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Vendor",
                columns: new[] { "Name", "CreateDate", "CreateGuid", "IsDeleted", "VendorGuid", "LoginUrl" },
                values: new object[] { "ITPro.TV", new DateTime(2019, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, new Guid("96229316-D51D-4A00-8FEB-E3EF0417C4EF"), "https://app.itpro.tv/login/" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCrawling",
                table: "CourseSite");

            migrationBuilder.DropColumn(
                name: "IsSyncing",
                table: "CourseSite");

            migrationBuilder.DeleteData(
                table: "Vendor",
                keyColumn: "Name",
                keyValue: "ITPro.TV");
        }
    }
}