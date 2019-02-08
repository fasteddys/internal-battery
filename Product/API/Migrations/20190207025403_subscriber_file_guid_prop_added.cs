using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscriber_file_guid_prop_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "SubscriberFile",
                nullable: false,
                defaultValueSql: "newid()");

            migrationBuilder.AddUniqueConstraint("subscriberFile_guid_unique", "SubscriberFile", "Guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint("subscriberFile_guid_unique", "SubscriberFile");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "SubscriberFile");
        }
    }
}
