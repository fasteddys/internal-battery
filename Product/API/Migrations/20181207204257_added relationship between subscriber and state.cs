using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrelationshipbetweensubscriberandstate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_StateId",
                table: "Subscriber",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriber_State_StateId",
                table: "Subscriber",
                column: "StateId",
                principalTable: "State",
                principalColumn: "StateId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriber_State_StateId",
                table: "Subscriber");

            migrationBuilder.DropIndex(
                name: "IX_Subscriber_StateId",
                table: "Subscriber");
        }
    }
}
