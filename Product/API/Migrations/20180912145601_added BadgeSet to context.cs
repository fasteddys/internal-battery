using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedBadgeSettocontext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BadgeSet",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    BadgeSetId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeSetGuid = table.Column<Guid>(nullable: true),
                    BadgeID = table.Column<int>(nullable: false),
                    BadgeSetName = table.Column<string>(nullable: false),
                    BadgeSetDescription = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: true),
                    SortOrder = table.Column<int>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeSet", x => x.BadgeSetId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BadgeSet");
        }
    }
}
