using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedtoGuidforExistingObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExistingObjectId",
                table: "ResumeParseResult");

            migrationBuilder.AddColumn<Guid>(
                name: "ExistingObjectGuid",
                table: "ResumeParseResult",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExistingObjectGuid",
                table: "ResumeParseResult");

            migrationBuilder.AddColumn<int>(
                name: "ExistingObjectId",
                table: "ResumeParseResult",
                nullable: false,
                defaultValue: 0);
        }
    }
}
