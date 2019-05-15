using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedjobapplicationstatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from JobApplication");

            migrationBuilder.AddColumn<int>(
                name: "JobApplicationStatusId",
                table: "JobApplication",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "JobApplicationStatus",
                columns: table => new
                {
                    JobApplicationStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobApplicationStatusGuid = table.Column<Guid>(nullable: false),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationStatus", x => x.JobApplicationStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplication_JobApplicationStatusId",
                table: "JobApplication",
                column: "JobApplicationStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_JobApplicationStatus_JobApplicationStatusId",
                table: "JobApplication",
                column: "JobApplicationStatusId",
                principalTable: "JobApplicationStatus",
                principalColumn: "JobApplicationStatusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.InsertData(
                table: "JobApplicationStatus",
                columns: new[] { "IsDeleted", "JobApplicationStatusGuid", "Status", "CreateDate", "CreateGuid" },
                values: new object[,]
                {
                    { 0, Guid.NewGuid(), "New",  new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "In Review", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "Interview", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "Offer Extended", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "Closed - Offer Accepted", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "Closed - Offer Rejected", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty},
                    { 0, Guid.NewGuid(), "Closed - No Offer", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty}
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_JobApplicationStatus_JobApplicationStatusId",
                table: "JobApplication");

            migrationBuilder.DropTable(
                name: "JobApplicationStatus");

            migrationBuilder.DropIndex(
                name: "IX_JobApplication_JobApplicationStatusId",
                table: "JobApplication");

            migrationBuilder.DropColumn(
                name: "JobApplicationStatusId",
                table: "JobApplication");
        }
    }
}
