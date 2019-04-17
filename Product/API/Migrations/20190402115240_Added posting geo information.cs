using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addedpostinggeoinformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "JobPosting",
                newName: "StreetAddress");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "JobPosting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "JobPosting",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "JobPosting");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "JobPosting");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "JobPosting",
                newName: "Location");
        }
    }
}
