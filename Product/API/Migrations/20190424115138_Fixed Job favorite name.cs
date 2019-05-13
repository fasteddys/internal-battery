using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class FixedJobfavoritename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingFavorites");

            migrationBuilder.CreateTable(
                name: "JobPostingFavorite",
                columns: table => new
                {
                    JobPostingFavoriteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    JobPostingFavoriteGuid = table.Column<Guid>(nullable: false),
                    JobPostingId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingFavorite", x => x.JobPostingFavoriteId);
                    table.ForeignKey(
                        name: "FK_JobPostingFavorite_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingFavorite_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingFavorite_JobPostingId",
                table: "JobPostingFavorite",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingFavorite_SubscriberId",
                table: "JobPostingFavorite",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingFavorite");

            migrationBuilder.CreateTable(
                name: "JobPostingFavorites",
                columns: table => new
                {
                    JobPostingFavoritesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    JobPostingFavoritesGuid = table.Column<Guid>(nullable: false),
                    JobPostingId = table.Column<int>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingFavorites", x => x.JobPostingFavoritesId);
                    table.ForeignKey(
                        name: "FK_JobPostingFavorites_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingFavorites_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingFavorites_JobPostingId",
                table: "JobPostingFavorites",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingFavorites_SubscriberId",
                table: "JobPostingFavorites",
                column: "SubscriberId");
        }
    }
}
