using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class JobViewCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Action",
            columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "ActionGuid", "Name", "Description" },
            values: new object[] { 0, DateTime.UtcNow, null, Guid.Empty, null, Guid.NewGuid(), "View", "Subscriber Viewed" }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "Action",
            keyColumn: "Name",
            keyValue: "View");
        }
    }
}
