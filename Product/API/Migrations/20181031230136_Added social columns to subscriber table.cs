using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addedsocialcolumnstosubscribertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GithubUrl",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StackOverflowUrl",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "GithubUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "StackOverflowUrl",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Subscriber");
        }
    }
}
