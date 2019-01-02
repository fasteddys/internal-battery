using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class AddCompensationTypeData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CompensationType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CompensationTypeGuid", "CompensationTypeName" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Hourly" }
            );

            migrationBuilder.InsertData(
              table: "CompensationType",
              columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CompensationTypeGuid", "CompensationTypeName" },
              values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Weekly" }
            );

            migrationBuilder.InsertData(
                table: "CompensationType",
                 columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CompensationTypeGuid", "CompensationTypeName" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Monthly" }
            );

            migrationBuilder.InsertData(
                 table: "CompensationType",
                 columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CompensationTypeGuid", "CompensationTypeName" },
                 values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Annual" }
            );

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [CompensationType]", true);
        }
    }
}
