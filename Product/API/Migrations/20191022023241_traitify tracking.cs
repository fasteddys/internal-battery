using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class traitifytracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "ActionGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    {  new Guid("8b292d84-df22-4198-ad5f-c0d82a7dfc99"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "Subscriber created an account through Traitify", 0, null, null, "Traitify Account Creation" },
                });

                migrationBuilder.InsertData(
                table: "EntityType",
                columns: new[] { "EntityTypeGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    {  new Guid("aff92565-a7ac-4555-ad3a-557265bf6c73"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "This is a reference to Traitify Assessment", 0, null, null, "Traitify Assessment" },
                });                

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionGuid",
                keyValue: "8b292d84-df22-4198-ad5f-c0d82a7dfc99");
            
            migrationBuilder.DeleteData(
                table: "EntityType",
                keyColumn: "EntityTypeGuid",
                keyValue: "aff92565-a7ac-4555-ad3a-557265bf6c73");
        }
    }
}
