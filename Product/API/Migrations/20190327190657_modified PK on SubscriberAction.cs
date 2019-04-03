using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifiedPKonSubscriberAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriberAction",
                table: "SubscriberAction");

            migrationBuilder.AddColumn<int>(
                name: "SubscriberActionId",
                table: "SubscriberAction",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriberAction",
                table: "SubscriberAction",
                column: "SubscriberActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_SubscriberId",
                table: "SubscriberAction",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriberAction",
                table: "SubscriberAction");

            migrationBuilder.DropIndex(
                name: "IX_SubscriberAction_SubscriberId",
                table: "SubscriberAction");

            migrationBuilder.DropColumn(
                name: "SubscriberActionId",
                table: "SubscriberAction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriberAction",
                table: "SubscriberAction",
                columns: new[] { "SubscriberId", "ActionId" });
        }
    }
}
