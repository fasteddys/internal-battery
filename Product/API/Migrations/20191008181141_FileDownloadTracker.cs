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
                    MaxFileDownloadAttemptsPermitted = table.Column<int>(nullable: true),
                    SourceFileCDNUrl = table.Column<string>(nullable: true),
                    MostrecentfiledownloadAttemptinUtc = table.Column<DateTime>(nullable: true),
                    SubscriberGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownloadTracker", x => x.FileDownloadTrackerId);
                });

                migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "ActionGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    {  new Guid("256f1626-b54e-4352-99ad-217bae5f88a7"), new DateTime(2019, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "Subscriber clicked on the gated download link in the email.", 0, null, null, "Download Gated File" },
                });

                migrationBuilder.InsertData(
                table: "EntityType",
                columns: new[] { "EntityTypeGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    {  new Guid("3c360a2a-ce00-4a1c-a962-0789a5acae32"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), "This is a reference to FileDownloadTracker entity", 0, null, null, "File Download Tracker" },
                });                
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDownloadTracker");

            migrationBuilder.DeleteData(
                table: "Action",
                keyColumn: "ActionGuid",
                keyValue: "256f1626-b54e-4352-99ad-217bae5f88a7");
            
            migrationBuilder.DeleteData(
                table: "EntityType",
                keyColumn: "EntityTypeGuid",
                keyValue: "3c360a2a-ce00-4a1c-a962-0789a5acae32");
        }
    }
}
