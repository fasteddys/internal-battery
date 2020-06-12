using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ProfileComment_Increase_Size : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "G2",
                table: "ProfileComments",
                maxLength: 2500,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 500);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "G2",
                table: "ProfileComments",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 2500);
        }
    }
}
