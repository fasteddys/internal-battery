using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class SubscriberTitleAndBiography : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Subscriber");
        }
    }
}
