using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addednoofferrebatetype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RebateType",
                columns: new[] { "RebateTypeId", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "RebateTypeGuid" },
                values: new object[] { 3, new DateTime(2019, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "There is no incentive for the subscriber to complete this course.", 0, null, null, "No offer", new Guid("5b8591f6-ad54-45a9-a319-56e4dbc1449e") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RebateType",
                keyColumn: "RebateTypeId",
                keyValue: 3);
        }
    }
}
