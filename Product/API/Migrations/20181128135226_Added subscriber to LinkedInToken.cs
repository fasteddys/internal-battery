using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedsubscribertoLinkedInToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "LinkedInToken");

            migrationBuilder.AddColumn<int>(
                name: "SubscriberId",
                table: "LinkedInToken",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LinkedInToken_SubscriberId",
                table: "LinkedInToken",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedInToken_Subscriber_SubscriberId",
                table: "LinkedInToken",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkedInToken_Subscriber_SubscriberId",
                table: "LinkedInToken");

            migrationBuilder.DropIndex(
                name: "IX_LinkedInToken_SubscriberId",
                table: "LinkedInToken");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                table: "LinkedInToken");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "LinkedInToken",
                nullable: true);
        }
    }
}
