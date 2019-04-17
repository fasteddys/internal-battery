using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingJobPostingSkilltoBaseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "JobPostingSkill",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "JobPostingSkill",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "JobPostingSkill",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "JobPostingSkill",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "JobPostingSkill",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "JobPostingSkill");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "JobPostingSkill");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JobPostingSkill");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "JobPostingSkill");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "JobPostingSkill");
        }
    }
}
