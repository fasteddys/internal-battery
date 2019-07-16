using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addinggooglecloudcolumnstosubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudTalentIndexInfo",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CloudTalentIndexStatus",
                table: "Subscriber",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CloudTalentUri",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudTalentIndexInfo",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CloudTalentIndexStatus",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CloudTalentUri",
                table: "Subscriber");
        }
    }
}
