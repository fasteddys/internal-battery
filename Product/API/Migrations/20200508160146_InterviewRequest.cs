using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class InterviewRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "B2B");

            migrationBuilder.CreateTable(
                name: "InterviewRequest",
                schema: "B2B",
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
                    HiringManagerId = table.Column<int>(nullable: true),
                    ProfileId = table.Column<int>(nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRequest_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRequest_HiringManagerId",
                schema: "B2B",
                table: "InterviewRequest",
                column: "HiringManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRequest_ProfileId",
                schema: "B2B",
                table: "InterviewRequest",
                column: "ProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewRequest",
                schema: "B2B");
        }
    }
}
