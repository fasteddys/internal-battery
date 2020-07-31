using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedDBchangesforUrltracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tracking",
                columns: table => new
                {
                    TrackingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    Url = table.Column<string>(type: "VARCHAR(2048)", nullable: false),
                    SourceSlug = table.Column<string>(type: "VARCHAR(50)", nullable: true),
                    TrackingGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracking", x => x.TrackingId);
                });

            migrationBuilder.CreateTable(
                name: "TrackingEventDay",
                columns: table => new
                {
                    TrackingEventDayId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TrackingEventDayGuid = table.Column<Guid>(nullable: false),
                    Day = table.Column<DateTime>(type: "Date", nullable: false),
                    Count = table.Column<int>(nullable: false),
                    TrackingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingEventDay", x => x.TrackingEventDayId);
                    table.ForeignKey(
                        name: "FK_TrackingEventDay_Tracking_TrackingId",
                        column: x => x.TrackingId,
                        principalTable: "Tracking",
                        principalColumn: "TrackingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEventDay_TrackingId",
                table: "TrackingEventDay",
                column: "TrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEventDay_Day",
                table: "TrackingEventDay",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_SourceSlug",
                table: "Tracking",
                column: "SourceSlug");

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_Url",
                table: "Tracking",
                column: "Url");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackingEventDay");

            migrationBuilder.DropTable(
                name: "Tracking");
        }
    }
}
