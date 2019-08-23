using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingMaxNumberOfRedemptionsPerSubscribercolumntoPromoCodemodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxNumberOfRedemptionsPerSubscriber",
                table: "PromoCode",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxNumberOfRedemptionsPerSubscriber",
                table: "PromoCode");
        }
    }
}
