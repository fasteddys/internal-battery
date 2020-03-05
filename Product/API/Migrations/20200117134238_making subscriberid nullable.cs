using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class makingsubscriberidnullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubscriberId",
                table: "SubscriberSendGridEvent",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubscriberId",
                table: "SubscriberSendGridEvent",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
