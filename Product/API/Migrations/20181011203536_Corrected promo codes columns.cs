using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Correctedpromocodescolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromoValueFacotr",
                table: "PromoCode",
                newName: "PromoValueFactor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromoValueFactor",
                table: "PromoCode",
                newName: "PromoValueFacotr");
        }
    }
}
