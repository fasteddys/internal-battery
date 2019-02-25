using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class contactschemachangesforbulkimport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partner",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PartnerGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partner", x => x.PartnerId);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContact",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerId = table.Column<int>(nullable: false),
                    ContactId = table.Column<int>(nullable: false),
                    PartnerContactGuid = table.Column<Guid>(nullable: true),
                    SourceSystemIdentifier = table.Column<string>(nullable: true),
                    MetaDataJSON = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContact", x => new { x.PartnerId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_PartnerContact_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContact_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Partner",
                columns: new[] { "IsDeleted", "PartnerGuid", "CreateDate", "CreateGuid", "Name", "Description" },
                values: new object[] { 0, Guid.Parse("C250AE21-2A81-4659-A05E-59DE90B12AF9"), DateTime.UtcNow, Guid.Empty, "Allegis Group", "Allegis Group, Inc. is an international talent management firm headquartered in Hanover, Maryland. As of 2016 it had US$11.2 billion in revenue. Founded as Aerotek in 1983 by Allegis Group's current CEO Jim C. Davis and Baltimore Ravens owner Steve Bisciotti, the company originally focused on the engineering and aerospace industry." }
                );

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContact_ContactId",
                table: "PartnerContact",
                column: "ContactId");

            migrationBuilder.Sql("ALTER TABLE dbo.PartnerContact ADD CONSTRAINT CK_MetaDataJSON_IsJSON CHECK (ISJSON(MetaDataJSON) > 0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerContact");

            migrationBuilder.DropTable(
                name: "Partner");
        }
    }
}
