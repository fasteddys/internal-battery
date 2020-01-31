using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class FixingNotificationGroupGuidStep1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationGroupGuid",
                table: "NotificationGroup");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationGroupGuid",
                table: "NotificationGroup",
                nullable: false,
                defaultValue: 0);
        }
    }
}
