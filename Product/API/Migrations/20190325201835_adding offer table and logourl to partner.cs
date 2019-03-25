using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingoffertableandlogourltopartner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Partner",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Offer",
                columns: table => new
                {
                    OfferId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    OfferGuid = table.Column<Guid>(nullable: false),
                    PartnerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Disclaimer = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offer", x => x.OfferId);
                    table.ForeignKey(
                        name: "FK_Offer_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offer_PartnerId",
                table: "Offer",
                column: "PartnerId");

            migrationBuilder.InsertData(
                table: "Offer",
                columns: new[] { "OfferId", "OfferGuid", "PartnerId", "Name", "Description", "Disclaimer", "Code", "Url", "StartDate", "EndDate", "CreateDate", "CreateGuid", "IsDeleted", "ModifyDate", "ModifyGuid" },
                values: new object[,]
                {
                    { 1, new Guid("411f6ed9-0f10-4e28-81bd-a7c8d888e84c"), 53, "ItProTv Test Offer", "ITProTv Test offer description", "Test disclaimer...", "TESTCode", "https://www.itpro.tv/", new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Offer");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Partner");
        }
    }
}
