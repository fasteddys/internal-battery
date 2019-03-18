using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingJobPostings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleCloudUri",
                table: "Company",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Industry",
                columns: table => new
                {
                    IndustryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IndustryGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industry", x => x.IndustryId);
                });

            migrationBuilder.CreateTable(
                name: "SecurityClearance",
                columns: table => new
                {
                    SecurityClearanceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SecurityClearanceGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityClearance", x => x.SecurityClearanceId);
                });

            migrationBuilder.CreateTable(
                name: "JobPosting",
                columns: table => new
                {
                    JobPostingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobPostingGuid = table.Column<Guid>(nullable: true),
                    PostingDateUTC = table.Column<DateTime>(nullable: false),
                    PostingExpirationDateUTC = table.Column<DateTime>(nullable: false),
                    GoogleCloudUri = table.Column<string>(nullable: true),
                    GoogleCloudIndexStatus = table.Column<int>(nullable: false),
                    CompanyGuid = table.Column<Guid>(nullable: false),
                    IndustryId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SecurityClearanceId = table.Column<int>(nullable: true),
                    H2Visa = table.Column<bool>(nullable: false),
                    TelecommutePercentage = table.Column<int>(nullable: false),
                    Compensation = table.Column<decimal>(nullable: false),
                    CompensationTypeId = table.Column<int>(nullable: true),
                    ThirdPartyApply = table.Column<bool>(nullable: false),
                    ThirdPartyApplicationUrl = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosting", x => x.JobPostingId);
                    table.ForeignKey(
                        name: "FK_JobPosting_Industry_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "Industry",
                        principalColumn: "IndustryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                columns: table => new
                {
                    EmploymentTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EmploymentTypeGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    JobPostingId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.EmploymentTypeId);
                    table.ForeignKey(
                        name: "FK_EmploymentType_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentType_JobPostingId",
                table: "EmploymentType",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_IndustryId",
                table: "JobPosting",
                column: "IndustryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmploymentType");

            migrationBuilder.DropTable(
                name: "SecurityClearance");

            migrationBuilder.DropTable(
                name: "JobPosting");

            migrationBuilder.DropTable(
                name: "Industry");

            migrationBuilder.DropColumn(
                name: "GoogleCloudUri",
                table: "Company");
        }
    }
}
