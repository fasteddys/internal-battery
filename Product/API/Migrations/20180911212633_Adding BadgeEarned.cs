using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingBadgeEarned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BadgeEarned",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: false),
                    BadgeEarnedID = table.Column<string>(nullable: false),
                    BadgeEarnedGuid = table.Column<string>(nullable: true),
                    BadgeID = table.Column<string>(nullable: false),
                    SubscriberID = table.Column<string>(nullable: true),
                    DateEarned = table.Column<DateTime>(nullable: false),
                    PointValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeEarned", x => x.BadgeEarnedID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BadgeEarned");
        }
    }
}
