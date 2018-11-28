using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class RemovedsubscribeGuidfromSubscriberProfileStagingStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriberGuid",
                table: "SubscriberProfileStagingStore");

            migrationBuilder.AddColumn<int>(
                name: "SubscriberId",
                table: "SubscriberProfileStagingStore",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberProfileStagingStore_SubscriberId",
                table: "SubscriberProfileStagingStore",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberProfileStagingStore_Subscriber_SubscriberId",
                table: "SubscriberProfileStagingStore",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberProfileStagingStore_Subscriber_SubscriberId",
                table: "SubscriberProfileStagingStore");

            migrationBuilder.DropIndex(
                name: "IX_SubscriberProfileStagingStore_SubscriberId",
                table: "SubscriberProfileStagingStore");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                table: "SubscriberProfileStagingStore");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberGuid",
                table: "SubscriberProfileStagingStore",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
