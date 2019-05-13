using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedJobSiteScrapeStatistic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobSiteScrapeStatistic",
                columns: table => new
                {
                    JobSiteScrapeStatisticId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobSiteScrapeStatisticGuid = table.Column<Guid>(nullable: false),
                    JobSiteId = table.Column<int>(nullable: false),
                    ScrapeDate = table.Column<DateTime>(nullable: false),
                    NumJobsProcessed = table.Column<int>(nullable: false),
                    NumJobsAdded = table.Column<int>(nullable: false),
                    NumJobsDropped = table.Column<int>(nullable: false),
                    NumJobsUpdated = table.Column<int>(nullable: false),
                    NumJobsErrored = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSiteScrapeStatistic", x => x.JobSiteScrapeStatisticId);
                    table.ForeignKey(
                        name: "FK_JobSiteScrapeStatistic_JobSite_JobSiteId",
                        column: x => x.JobSiteId,
                        principalTable: "JobSite",
                        principalColumn: "JobSiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSiteScrapeStatistic_JobSiteId",
                table: "JobSiteScrapeStatistic",
                column: "JobSiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSiteScrapeStatistic");
        }
    }
}
