using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addedpromocodetoserviceofferingorder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PromoCodeId",
                table: "ServiceOfferingOrder",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferingOrder_PromoCodeId",
                table: "ServiceOfferingOrder",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder",
                column: "PromoCodeId",
                principalTable: "PromoCode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOfferingOrder_PromoCodeId",
                table: "ServiceOfferingOrder");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                table: "ServiceOfferingOrder");
        }
    }
}
