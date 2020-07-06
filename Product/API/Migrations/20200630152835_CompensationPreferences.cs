using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class CompensationPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentRate",
                table: "Subscriber",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentSalary",
                table: "Subscriber",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DesiredRate",
                table: "Subscriber",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DesiredSalary",
                table: "Subscriber",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRate",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CurrentSalary",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "DesiredRate",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "DesiredSalary",
                table: "Subscriber");
        }
    }
}
