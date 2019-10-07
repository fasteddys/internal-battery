using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class FileDownloadTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    MaxFileDownloadAttemptsPermitted = table.Column<int>(nullable: false),
                    SourceFileCDNUrl = table.Column<string>(nullable: true),
                    MostrecentfiledownloadAttemptinUtc = table.Column<DateTime>(nullable: true),
                    SubscriberGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownloadTracker", x => x.FileDownloadTrackerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDownloadTracker");
        }
    }
}
