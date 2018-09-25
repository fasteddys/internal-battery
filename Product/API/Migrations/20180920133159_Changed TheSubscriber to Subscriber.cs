using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedTheSubscribertoSubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SubscriberId",
                table: "Enrollment",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Subscriber_SubscriberId",
                table: "Enrollment",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Subscriber_SubscriberId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_SubscriberId",
                table: "Enrollment");
        }
    }
}
