using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedjobapplicationstatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { 0, new Guid("80953e49-666c-4080-a180-3091c48773b2"), "New",  new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000")},
                    { 0, new Guid("0ef64333-56e0-4a02-bbef-caa625d3dda9"), "In Review", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 0, new Guid("55335c65-f7b0-4599-9ac5-6224f0fed45f"), "Interview", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 0, new Guid("a27bbe95-ece8-4fe8-b4f5-0507e82473da"), "Offer Extended", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 0, new Guid("bf074730-e0cb-4948-8e2f-0b8c2b6da6ee"), "Closed - Offer Accepted", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 0, new Guid("fa4ba64d-57d5-42a2-aaba-309929ff2f11"), "Closed - Offer Rejected", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 0, new Guid("98f054be-9f92-4431-b9a8-5959c5dda747"), "Closed - No Offer", new DateTime(2019, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") }
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
