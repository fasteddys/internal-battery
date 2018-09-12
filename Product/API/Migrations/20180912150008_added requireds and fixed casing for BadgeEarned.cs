using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsandfixedcasingforBadgeEarned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriberID",
                table: "BadgeEarned",
                newName: "SubscriberId");

            migrationBuilder.RenameColumn(
                name: "BadgeID",
                table: "BadgeEarned",
                newName: "BadgeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeEarnedGuid",
                table: "BadgeEarned",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "BadgeEarned",
                newName: "SubscriberID");

            migrationBuilder.RenameColumn(
                name: "BadgeId",
                table: "BadgeEarned",
                newName: "BadgeID");

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeEarnedGuid",
                table: "BadgeEarned",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
