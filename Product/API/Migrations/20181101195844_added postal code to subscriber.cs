using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedpostalcodetosubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "Subscriber",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Subscriber",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Subscriber");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "Subscriber",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
