using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscriberFile_change_column_names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "SubscriberFile",
                newName: "SubscriberGuid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SubscriberFile",
                newName: "SubscriberFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriberGuid",
                table: "SubscriberFile",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "SubscriberFileId",
                table: "SubscriberFile",
                newName: "Id");
        }
    }
}
