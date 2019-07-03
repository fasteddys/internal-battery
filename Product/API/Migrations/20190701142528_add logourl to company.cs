using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addlogourltocompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Company",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "CompanyName",
                keyValue: "Aerotek",
                column: "LogoUrl",
                value: "7E1D8AB0-3440-4773-88B6-2722DA9F2FED/aerotek.jpg");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "CompanyName",
                keyValue: "Allegis Group",
                column: "LogoUrl",
                value: "92728544-FDFD-493F-A7D3-5547DEA7B9DD/allegisgroup.png");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "CompanyName",
                keyValue: "TEKsystems",
                column: "LogoUrl",
                value: "2C2BC0D6-416B-4B62-A16A-87AC9B95D0B2/teksystems.jpg");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Company");
        }
    }
}
