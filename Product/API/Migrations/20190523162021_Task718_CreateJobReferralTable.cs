using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Task718_CreateJobReferralTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobReferral",
                columns: table => new
                {
                    JobReferralId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobReferralGuid = table.Column<Guid>(nullable: false),
                    JobPostingId = table.Column<int>(nullable: false),
                    ReferralId = table.Column<int>(nullable: false),
                    RefereeId = table.Column<int>(nullable: true),
                    RefereeEmailId = table.Column<string>(nullable: true),
                    IsJobViewed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobReferral", x => x.JobReferralId);
                    table.ForeignKey(
                        name: "FK_JobReferral_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobReferral_Subscriber_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobReferral_Subscriber_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobReferral_JobPostingId",
                table: "JobReferral",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobReferral_RefereeId",
                table: "JobReferral",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobReferral_ReferralId",
                table: "JobReferral",
                column: "ReferralId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobReferral");
        }
    }
}
