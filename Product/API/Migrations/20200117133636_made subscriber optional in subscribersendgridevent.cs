using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class madesubscriberoptionalinsubscribersendgridevent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberSendGridEvent_Subscriber_SubscriberId",
                table: "SubscriberSendGridEvent");

            migrationBuilder.DropIndex(
                name: "IX_SubscriberSendGridEvent_SubscriberId",
                table: "SubscriberSendGridEvent");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SubscriberSendGridEvent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "SubscriberSendGridEvent");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberSendGridEvent_SubscriberId",
                table: "SubscriberSendGridEvent",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberSendGridEvent_Subscriber_SubscriberId",
                table: "SubscriberSendGridEvent",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
