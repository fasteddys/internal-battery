using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingJobCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAgencyJobPosting",
                table: "JobPosting",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobCategoryId",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobCategory",
                columns: table => new
                {
                    JobCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobCategoryGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCategory", x => x.JobCategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosting_JobCategoryId",
                table: "JobPosting",
                column: "JobCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_JobCategory_JobCategoryId",
                table: "JobPosting",
                column: "JobCategoryId",
                principalTable: "JobCategory",
                principalColumn: "JobCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_JobCategory_JobCategoryId",
                table: "JobPosting");

            migrationBuilder.DropTable(
                name: "JobCategory");

            migrationBuilder.DropIndex(
                name: "IX_JobPosting_JobCategoryId",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "IsAgencyJobPosting",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "JobCategoryId",
                table: "JobPosting");
        }
    }
}
