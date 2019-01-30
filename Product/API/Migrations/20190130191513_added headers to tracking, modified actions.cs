using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class addedheaderstotrackingmodifiedactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Headers",
                table: "ContactAction",
                nullable: true);
            
            migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "IsDeleted", "ActionGuid", "CreateDate", "CreateGuid", "Name" },
                values: new object[] { 0, Guid.Parse("CCBEE3A5-278E-4696-9CB6-AB6DC5B50D0A"), DateTime.UtcNow, Guid.Empty, "Complete course" }
                );

            migrationBuilder.UpdateData(
                table: "Action",
                keyColumn: "ActionGuid",
                keyValue: Guid.Parse("47D62280-213F-44F3-8085-A83BB2A5BBE3"),
                column: "Name",
                value: "Visit landing page"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Headers",
                table: "ContactAction");
        }
    }
}
