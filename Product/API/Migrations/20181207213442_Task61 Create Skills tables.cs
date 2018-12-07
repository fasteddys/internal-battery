using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Task61CreateSkillstables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SkillId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SkillName = table.Column<string>(nullable: true),
                    ONetCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberSkill",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberSkillId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberId = table.Column<int>(nullable: false),
                    SkillId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberSkill", x => x.SubscriberSkillId);
                    table.ForeignKey(
                        name: "FK_SubscriberSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberSkill_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skill_SkillName",
                table: "Skill",
                column: "SkillName",
                unique: true,
                filter: "[SkillName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberSkill_SkillId",
                table: "SubscriberSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberSkill_SubscriberId",
                table: "SubscriberSkill",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberSkill");

            migrationBuilder.DropTable(
                name: "Skill");
        }
    }
}
