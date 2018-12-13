using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedguidtoskillandsubscriberskillremovedONetCodefromskill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Skill_SkillName",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "ONetCode",
                table: "Skill");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberSkillGuid",
                table: "SubscriberSkill",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SkillName",
                table: "Skill",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillGuid",
                table: "Skill",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skill_SkillName",
                table: "Skill",
                column: "SkillName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Skill_SkillName",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "SubscriberSkillGuid",
                table: "SubscriberSkill");

            migrationBuilder.DropColumn(
                name: "SkillGuid",
                table: "Skill");

            migrationBuilder.AlterColumn<string>(
                name: "SkillName",
                table: "Skill",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "ONetCode",
                table: "Skill",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skill_SkillName",
                table: "Skill",
                column: "SkillName",
                unique: true,
                filter: "[SkillName] IS NOT NULL");
        }
    }
}
