using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ResumeParseResultmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResumeParseResult",
                columns: table => new
                {
                    ResumeParseResultId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ResumeParseResultGuid = table.Column<Guid>(nullable: false),
                    ResumeParseId = table.Column<int>(nullable: false),
                    ParseStatus = table.Column<int>(nullable: false),
                    TargetTypeName = table.Column<string>(nullable: true),
                    TargetProperty = table.Column<string>(nullable: true),
                    ParsedValue = table.Column<string>(nullable: true),
                    ExistingValue = table.Column<string>(nullable: true),
                    ExistingObjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumeParseResult", x => x.ResumeParseResultId);
                    table.ForeignKey(
                        name: "FK_ResumeParseResult_ResumeParse_ResumeParseId",
                        column: x => x.ResumeParseId,
                        principalTable: "ResumeParse",
                        principalColumn: "ResumeParseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResumeParseResult_ResumeParseId",
                table: "ResumeParseResult",
                column: "ResumeParseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumeParseResult");
        }
    }
}
