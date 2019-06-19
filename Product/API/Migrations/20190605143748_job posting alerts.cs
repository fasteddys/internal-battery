using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobpostingalerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostingAlert",
                columns: table => new
                {
                    JobPostingAlertId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobPostingAlertGuid = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    ExecutionHour = table.Column<int>(nullable: false),
                    ExecutionMinute = table.Column<int>(nullable: false),
                    JobSearchDtoJSON = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingAlert", x => x.JobPostingAlertId);
                    table.ForeignKey(
                        name: "FK_JobPostingAlert_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingAlert_SubscriberId",
                table: "JobPostingAlert",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingAlert");
        }
    }
}
