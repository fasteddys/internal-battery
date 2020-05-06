using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class _2188_InterviewRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "B2B");

            migrationBuilder.RenameTable(
                name: "HiringManagers",
                schema: "G2",
                newName: "HiringManagers",
                newSchema: "B2B");

            migrationBuilder.CreateTable(
                name: "InterviewRequest",
                columns: table => new
                {
                    InterviewRequestId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    InterviewRequestGuid = table.Column<Guid>(nullable: false),
                    HiringManagerId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    DateRequested = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRequest", x => x.InterviewRequestId);
                    table.ForeignKey(
                        name: "FK_InterviewRequest_HiringManagers_HiringManagerId",
                        column: x => x.HiringManagerId,
                        principalSchema: "B2B",
                        principalTable: "HiringManagers",
                        principalColumn: "HiringManagerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewRequest_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRequest_HiringManagerId",
                table: "InterviewRequest",
                column: "HiringManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRequest_ProfileId",
                table: "InterviewRequest",
                column: "ProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewRequest");

            migrationBuilder.RenameTable(
                name: "HiringManagers",
                schema: "B2B",
                newName: "HiringManagers",
                newSchema: "G2");
        }
    }
}
