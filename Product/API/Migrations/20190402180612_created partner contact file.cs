using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class createdpartnercontactfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerContactFile",
                columns: table => new
                {
                    PartnerContactFileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerContactFileGuid = table.Column<Guid>(nullable: false),
                    PartnerContactId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    MimeType = table.Column<string>(nullable: false),
                    Base64EncodedData = table.Column<string>(unicode: false, nullable: false),
                    IsBillable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactFile", x => x.PartnerContactFileId);
                    table.ForeignKey(
                        name: "FK_PartnerContactFile_PartnerContact_PartnerContactId",
                        column: x => x.PartnerContactId,
                        principalTable: "PartnerContact",
                        principalColumn: "PartnerContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContactFileLeadStatus",
                columns: table => new
                {
                    PartnerContactId = table.Column<int>(nullable: false),
                    LeadStatusId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerContactFileLeadStatusGuid = table.Column<Guid>(nullable: true),
                    PartnerContactFileId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactFileLeadStatus", x => new { x.PartnerContactId, x.LeadStatusId });
                    table.ForeignKey(
                        name: "FK_PartnerContactFileLeadStatus_LeadStatus_LeadStatusId",
                        column: x => x.LeadStatusId,
                        principalTable: "LeadStatus",
                        principalColumn: "LeadStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContactFileLeadStatus_PartnerContactFile_PartnerContactFileId",
                        column: x => x.PartnerContactFileId,
                        principalTable: "PartnerContactFile",
                        principalColumn: "PartnerContactFileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerContactFileLeadStatus_PartnerContact_PartnerContactId",
                        column: x => x.PartnerContactId,
                        principalTable: "PartnerContact",
                        principalColumn: "PartnerContactId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactFile_PartnerContactId",
                table: "PartnerContactFile",
                column: "PartnerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactFileLeadStatus_LeadStatusId",
                table: "PartnerContactFileLeadStatus",
                column: "LeadStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactFileLeadStatus_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                column: "PartnerContactFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerContactFileLeadStatus");

            migrationBuilder.DropTable(
                name: "PartnerContactFile");
        }
    }
}
