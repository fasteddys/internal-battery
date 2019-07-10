using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class JobAbandonment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
              table: "EntityType",
              columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EntityTypeGuid", "Name", "Description" },
              values: new object[] { 0, DateTime.UtcNow, null, Guid.Empty, null, Guid.NewGuid(), "Job posting", "" }
              );

            migrationBuilder.InsertData(
             table: "Action",
             columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "ActionGuid", "Name", "Description" },
             values: new object[] { 0, DateTime.UtcNow, null, Guid.Empty, null, Guid.NewGuid(), "Apply job", "Subscriber clicked on Apply" }
             );

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
              table: "EntityType",
              keyColumn: "Name",
              keyValue: "Job Posting");

            migrationBuilder.DeleteData(
           table: "Action",
           keyColumn: "Name",
           keyValue: "Apply Job");
        }
    }
}
