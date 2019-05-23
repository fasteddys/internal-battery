using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class zerobounceintegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
              table: "LeadStatus",
              columns: new[] { "LeadStatusId", "LeadStatusGuid", "Name", "Description", "Severity", "IsDeleted", "CreateDate", "CreateGuid" },
              values: new object[,]
              {
                    { 7, new Guid("E2FA5C17-E07D-4679-86A1-996262DC9525"), "Verification Failure", "The lead failed a third party verification process", "Rejected", 0, new DateTime(2019, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") }
              });

            migrationBuilder.CreateTable(
                name: "ZeroBounce",
                columns: table => new
                {
                    ZeroBounceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ZeroBounceGuid = table.Column<Guid>(nullable: false),
                    HttpStatus = table.Column<string>(nullable: true),
                    ElapsedTimeInMilliseconds = table.Column<int>(nullable: false),
                    PartnerContactId = table.Column<int>(nullable: true),
                    ResponseJSON = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZeroBounce", x => x.ZeroBounceId);
                    table.ForeignKey(
                        name: "FK_ZeroBounce_PartnerContact_PartnerContactId",
                        column: x => x.PartnerContactId,
                        principalTable: "PartnerContact",
                        principalColumn: "PartnerContactId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZeroBounce_PartnerContactId",
                table: "ZeroBounce",
                column: "PartnerContactId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZeroBounce");
        }
    }
}
