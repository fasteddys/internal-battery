using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class addentitytypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EntityType",
                columns: new[] { "EntityTypeId", "EntityTypeGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    { 1, new Guid("31CF84FA-4621-4C27-A45D-37078029FA85"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Subscriber" },
                    { 2, new Guid("DFE2B34A-D5AC-4D89-AA7A-3A7D3318A3DC"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Offer" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
