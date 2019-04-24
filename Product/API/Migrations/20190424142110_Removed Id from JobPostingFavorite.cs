using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class RemovedIdfromJobPostingFavorite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobPostingFavorite",
                table: "JobPostingFavorite");

            migrationBuilder.DropIndex(
                name: "IX_JobPostingFavorite_JobPostingId",
                table: "JobPostingFavorite");

            migrationBuilder.DropColumn(
                name: "JobPostingFavoriteId",
                table: "JobPostingFavorite");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobPostingFavorite",
                table: "JobPostingFavorite",
                columns: new[] { "JobPostingId", "SubscriberId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobPostingFavorite",
                table: "JobPostingFavorite");

            migrationBuilder.AddColumn<int>(
                name: "JobPostingFavoriteId",
                table: "JobPostingFavorite",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobPostingFavorite",
                table: "JobPostingFavorite",
                column: "JobPostingFavoriteId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingFavorite_JobPostingId",
                table: "JobPostingFavorite",
                column: "JobPostingId");
        }
    }
}
