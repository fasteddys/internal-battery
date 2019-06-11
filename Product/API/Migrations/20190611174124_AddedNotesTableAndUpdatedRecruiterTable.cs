using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedNotesTableAndUpdatedRecruiterTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Recruiter",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriberNotes",
                columns: table => new
                {
                    SubscriberNotesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberNotesGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    RecruiterId = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    ViewableByOthersInRecruiterCompany = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberNotes", x => x.SubscriberNotesId);
                    table.ForeignKey(
                        name: "FK_SubscriberNotes_Recruiter_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiter",
                        principalColumn: "RecruiterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberNotes_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiter_CompanyId",
                table: "Recruiter",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberNotes_RecruiterId",
                table: "SubscriberNotes",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberNotes_SubscriberId",
                table: "SubscriberNotes",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recruiter_Company_CompanyId",
                table: "Recruiter",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recruiter_Company_CompanyId",
                table: "Recruiter");

            migrationBuilder.DropTable(
                name: "SubscriberNotes");

            migrationBuilder.DropIndex(
                name: "IX_Recruiter_CompanyId",
                table: "Recruiter");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Recruiter");
        }
    }
}
