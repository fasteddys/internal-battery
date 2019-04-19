using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSkillstoJobPosting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostingSkill",
                columns: table => new
                {
                    JobPostingId = table.Column<int>(nullable: false),
                    SkillId = table.Column<int>(nullable: false),
                    JobPostingSkillGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingSkill", x => new { x.SkillId, x.JobPostingId });
                    table.ForeignKey(
                        name: "FK_JobPostingSkill_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingSkill_JobPostingId",
                table: "JobPostingSkill",
                column: "JobPostingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingSkill");
        }
    }
}
