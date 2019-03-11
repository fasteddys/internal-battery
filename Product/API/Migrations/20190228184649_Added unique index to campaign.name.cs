using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addeduniqueindextocampaignname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Campaign",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Campaign_Name",
                table: "Campaign",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaign_Name",
                table: "Campaign");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Campaign",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
