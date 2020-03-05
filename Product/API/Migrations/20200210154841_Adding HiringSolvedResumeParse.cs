using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingHiringSolvedResumeParse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HiringSolvedResumeParse",
                columns: table => new
                {
                    HiringSolvedResumeParseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    HiringSolvedResumeParseGuid = table.Column<Guid>(nullable: false),
                    ParseRequested = table.Column<DateTime>(nullable: true),
                    ParseCompleted = table.Column<DateTime>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false),
                    ParseStatus = table.Column<string>(nullable: true),
                    JobId = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    ResumeText = table.Column<string>(nullable: true),
                    ParsedResume = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringSolvedResumeParse", x => x.HiringSolvedResumeParseId);
                    table.ForeignKey(
                        name: "FK_HiringSolvedResumeParse_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiringSolvedResumeParse_SubscriberId",
                table: "HiringSolvedResumeParse",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringSolvedResumeParse");
        }
    }
}
