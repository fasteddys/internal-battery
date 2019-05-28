using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingavatarstosubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInAvatarUrl",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "LinkedInAvatarUrl",
                table: "Subscriber");
        }
    }
}
