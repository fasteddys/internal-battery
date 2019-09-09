using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class MadePromoCodeIdcolumnnullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder");

            migrationBuilder.AlterColumn<int>(
                name: "PromoCodeId",
                table: "ServiceOfferingOrder",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder",
                column: "PromoCodeId",
                principalTable: "PromoCode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder");

            migrationBuilder.AlterColumn<int>(
                name: "PromoCodeId",
                table: "ServiceOfferingOrder",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOfferingOrder_PromoCode_PromoCodeId",
                table: "ServiceOfferingOrder",
                column: "PromoCodeId",
                principalTable: "PromoCode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
