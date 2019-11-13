using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingGroupIdtoFileDownloadTrackertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDownloadTracker");

            migrationBuilder.CreateTable(
                name: "FileDownloadTracker",
                columns: table => new
                {
                    FileDownloadTrackerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    FileDownloadTrackerGuid = table.Column<Guid>(nullable: true),
                    FileDownloadAttemptCount = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: true),
                    MaxFileDownloadAttemptsPermitted = table.Column<int>(nullable: true),
                    SourceFileCDNUrl = table.Column<string>(nullable: true),
                    MostrecentfiledownloadAttemptinUtc = table.Column<DateTime>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownloadTracker", x => x.FileDownloadTrackerId);
                    table.ForeignKey(
                        name: "FK_FileDownloadTracker_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDownloadTracker_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileDownloadTracker_GroupId",
                table: "FileDownloadTracker",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDownloadTracker_SubscriberId",
                table: "FileDownloadTracker",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDownloadTracker");
        }
    }
}
