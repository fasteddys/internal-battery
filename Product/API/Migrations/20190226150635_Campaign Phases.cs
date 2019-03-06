using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class CampaignPhases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                  name: "CampaignPhase",
                  columns: table => new
                  {
                      IsDeleted = table.Column<int>(nullable: false),
                      CreateDate = table.Column<DateTime>(nullable: false),
                      ModifyDate = table.Column<DateTime>(nullable: true),
                      CreateGuid = table.Column<Guid>(nullable: false),
                      ModifyGuid = table.Column<Guid>(nullable: true),
                      CampaignPhaseId = table.Column<int>(nullable: false)
                          .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                      CampaignPhaseGuid = table.Column<Guid>(nullable: false),
                      Name = table.Column<string>(nullable: false),
                      Description = table.Column<string>(nullable: true),
                      CampaignId = table.Column<int>(nullable: false)
                  },
                  constraints: table =>
                  {
                      table.PrimaryKey("PK_CampaignPhase", x => x.CampaignPhaseId);
                      table.ForeignKey(
                          name: "FK_CampaignPhase_Campaign_CampaignId",
                          column: x => x.CampaignId,
                          principalTable: "Campaign",
                          principalColumn: "CampaignId",
                          onDelete: ReferentialAction.NoAction);
                  });



            migrationBuilder.AddColumn<int>(
                name: "CampaignPhaseId",
                table: "ContactAction",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.Sql(@"DECLARE @CampaignPhases TABLE(CampaignPhaseID INT, CampaignId INT)
INSERT INTO CampaignPhase(IsDeleted, CreateDate, CreateGuid, CampaignPhaseGuid, [Name], [Description], CampaignId)
OUTPUT inserted.CampaignPhaseId, inserted.CampaignId into @CampaignPhases
SELECT 0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', NEWID(), c.Name + ' - Phase 1'[Name], 'Auto-generated value before we introduced phases'[Description], c.CampaignId
FROM campaign c
SELECT *
FROM @CampaignPhases
UPDATE ca
SET ca.CampaignPhaseID = cp.CampaignPhaseID
FROM dbo.ContactAction ca
INNER JOIN @CampaignPhases cp ON ca.CampaignId = cp.CampaignId");



            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_CampaignPhaseId",
                table: "ContactAction",
                column: "CampaignPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPhase_CampaignId",
                table: "CampaignPhase",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactAction_CampaignPhase_CampaignPhaseId",
                table: "ContactAction",
                column: "CampaignPhaseId",
                principalTable: "CampaignPhase",
                principalColumn: "CampaignPhaseId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactAction",
                table: "ContactAction");

            migrationBuilder.DropIndex(
                name: "IX_ContactAction_ActionId",
                table: "ContactAction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactAction",
                table: "ContactAction",
                columns: new[] { "ContactId", "CampaignId", "ActionId", "CampaignPhaseId" });

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactAction_CampaignPhase_CampaignPhaseId",
                table: "ContactAction");

            migrationBuilder.DropTable(
                name: "CampaignPhase");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ContactAction_ActionId_CampaignId_CampaignPhaseId_ContactId",
                table: "ContactAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactAction",
                table: "ContactAction");

            migrationBuilder.DropIndex(
                name: "IX_ContactAction_CampaignPhaseId",
                table: "ContactAction");

            migrationBuilder.DropColumn(
                name: "CampaignPhaseId",
                table: "ContactAction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactAction",
                table: "ContactAction",
                columns: new[] { "ContactId", "CampaignId", "ActionId" });

            migrationBuilder.CreateTable(
                name: "CampaignDetail",
                columns: table => new
                {
                    CourseName = table.Column<string>(nullable: false),
                    CourseCompletion = table.Column<int>(nullable: false),
                    CourseEnrollment = table.Column<int>(nullable: false),
                    CreateAcount = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    OpenEmail = table.Column<int>(nullable: false),
                    Rebatetype = table.Column<string>(nullable: true),
                    VisitLandingPage = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignDetail", x => x.CourseName);
                });

            migrationBuilder.CreateTable(
                name: "CampaignStatistic",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    CampaignGuid = table.Column<Guid>(nullable: false),
                    CourseCompletion = table.Column<int>(nullable: false),
                    CourseEnrollment = table.Column<int>(nullable: false),
                    CreateAcount = table.Column<int>(nullable: false),
                    EmailsSent = table.Column<int>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    OpenEmail = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    VisitLandingPage = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStatistic", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactAction_ActionId",
                table: "ContactAction",
                column: "ActionId");
        }
    }
}
