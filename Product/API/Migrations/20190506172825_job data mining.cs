using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobdatamining : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPageStatus",
                columns: table => new
                {
                    JobPageStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobPageStatusGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPageStatus", x => x.JobPageStatusId);
                });

            migrationBuilder.CreateTable(
                name: "JobSite",
                columns: table => new
                {
                    JobSiteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobSiteGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSite", x => x.JobSiteId);
                });

            migrationBuilder.InsertData(
                table: "JobSite",
                columns: new[] { "JobSiteGuid", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Uri" },
                values: new object[] { new Guid("D9C816C6-C6F9-4E23-981D-6BA29E83A48E"), new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "TEKsystems", "https://www.teksystems.com/it-jobs/api/results?geoIp=false" });

            migrationBuilder.InsertData(
                table: "JobSite",
                columns: new[] { "JobSiteGuid", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Uri" },
                values: new object[] { new Guid("034C10D5-FFFF-4CBD-BDDC-AF4B8460BFA3"), new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Aerotek", "https://www.aerotek.com/jobs/api/results?geoIp=false" });

            migrationBuilder.CreateTable(
                name: "JobPage",
                columns: table => new
                {
                    JobPageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobPageGuid = table.Column<Guid>(nullable: false),
                    Uri = table.Column<string>(nullable: true),
                    JobPageStatusId = table.Column<int>(nullable: false),
                    JobPostingId = table.Column<int>(nullable: true),
                    JobSiteId = table.Column<int>(nullable: false),
                    UniqueIdentifier = table.Column<string>(nullable: true),
                    RawData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPage", x => x.JobPageId);
                    table.ForeignKey(
                        name: "FK_JobPage_JobPageStatus_JobPageStatusId",
                        column: x => x.JobPageStatusId,
                        principalTable: "JobPageStatus",
                        principalColumn: "JobPageStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPage_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPage_JobSite_JobSiteId",
                        column: x => x.JobSiteId,
                        principalTable: "JobSite",
                        principalColumn: "JobSiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPage_JobPageStatusId",
                table: "JobPage",
                column: "JobPageStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPage_JobPostingId",
                table: "JobPage",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPage_JobSiteId",
                table: "JobPage",
                column: "JobSiteId");

            migrationBuilder.CreateIndex(
                name: "UIX_JobPage_JobSite_UniqueIdentifier",
                table: "JobPage",
                columns: new[] { "UniqueIdentifier", "JobSiteId" },
                unique: true,
                filter: "[UniqueIdentifier] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UIX_JobSite_Name",
                table: "JobSite",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.InsertData(
                table: "JobPageStatus",
                columns: new[] { "JobPageStatusGuid", "JobPageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("892D6662-2EC2-41E9-8CB1-6983A1893AC5"), 1, new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Pending", "This status is applied to a JobPage when raw data has been inserted or modified. This occurs before processing occurs to act on an associated JobPosting." });

            migrationBuilder.InsertData(
                table: "JobPageStatus",
                columns: new[] { "JobPageStatusGuid", "JobPageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("F20DD80D-D684-42DC-84A9-F47D22A1A33F"), 2, new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Active", "This status is applied to a JobPage when after a job page has been processed successfully and associated with an active JobPosting." });

            migrationBuilder.InsertData(
                table: "JobPageStatus",
                columns: new[] { "JobPageStatusGuid", "JobPageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("5849C93F-8407-4A8D-B4FB-CEDC11CB94CE"), 3, new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Error", "This status is applied to a JobPage when an error occurs during JobPage parsing which prevents an associated JobPosting record from being inserted or updated." });

            migrationBuilder.InsertData(
                table: "JobPageStatus",
                columns: new[] { "JobPageStatusGuid", "JobPageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("C0AB82C7-0443-4391-860D-2C972C480353"), 4, new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Deleted", "This status is applied to a JobPage when it is discovered that a previously seen job is no longer visible on a JobSite." });

            /* don't think we need this if duplicates are dealt with before they make it into the db
            migrationBuilder.InsertData(
                table: "JobPageStatus",
                columns: new[] { "JobPageStatusGuid", "JobPageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("5849C93F-8407-4A8D-B4FB-CEDC11CB94CE"), 5, new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Duplicate", "This status is applied to a JobPage when it is identified as a duplicate of another JobPage for the same JobSite." });
            */

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "CompanyGuid", "CreateDate", "CreateGuid", "IsDeleted", "CompanyName", "CloudTalentIndexStatus", "IsHiringAgency", "IsJobPoster" },
                values: new object[] { new Guid("2C2BC0D6-416B-4B62-A16A-87AC9B95D0B2"), new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "TEKsystems", 0, 1, 1 });

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "CompanyGuid", "CreateDate", "CreateGuid", "IsDeleted", "CompanyName", "CloudTalentIndexStatus", "IsHiringAgency", "IsJobPoster" },
                values: new object[] { new Guid("7E1D8AB0-3440-4773-88B6-2722DA9F2FED"), new DateTime(2019, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Aerotek", 0, 1, 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPage");

            migrationBuilder.DropTable(
                name: "JobPageStatus");

            migrationBuilder.DropTable(
                name: "JobSite");
        }
    }
}
