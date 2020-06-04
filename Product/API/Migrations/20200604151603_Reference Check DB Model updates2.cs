using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ReferenceCheckDBModelupdates2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceCheckReportFile",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropColumn(
                name: "ReferenceCheckReportFileUrl",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.CreateTable(
                name: "ReferenceCheckReport",
                schema: "G2",
                columns: table => new
                {
                    ReferenceCheckReportId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ReferenceCheckReportGuid = table.Column<Guid>(nullable: false),
                    Base64File = table.Column<string>(type: "Varchar(MAX)", nullable: true),
                    FileUrl = table.Column<string>(nullable: false),
                    FileType = table.Column<string>(maxLength: 25, nullable: false),
                    ReferenceCheckId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCheckReport", x => x.ReferenceCheckReportId);
                    table.ForeignKey(
                        name: "FK_ReferenceCheckReport_ReferenceCheck_ReferenceCheckId",
                        column: x => x.ReferenceCheckId,
                        principalSchema: "G2",
                        principalTable: "ReferenceCheck",
                        principalColumn: "ReferenceCheckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheckReport_ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckReport",
                column: "ReferenceCheckId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceCheckReport",
                schema: "G2");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCheckReportFile",
                schema: "G2",
                table: "ReferenceCheck",
                type: "Varchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCheckReportFileUrl",
                schema: "G2",
                table: "ReferenceCheck",
                nullable: true);
        }
    }
}
