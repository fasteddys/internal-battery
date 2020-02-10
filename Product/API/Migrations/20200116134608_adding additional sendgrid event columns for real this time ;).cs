using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingadditionalsendgrideventcolumnsforrealthistime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marketing_campaign_id",
                table: "SendGridEvent",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Marketing_campaign_name",
                table: "SendGridEvent",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "SendGridEvent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marketing_campaign_id",
                table: "SendGridEvent");

            migrationBuilder.DropColumn(
                name: "Marketing_campaign_name",
                table: "SendGridEvent");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "SendGridEvent");
        }
    }
}
