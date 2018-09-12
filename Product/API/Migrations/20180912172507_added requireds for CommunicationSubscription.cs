using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforCommunicationSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunicationSubscriptionGuid",
                table: "CommunicationSubscription",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunicationSubscriptionGuid",
                table: "CommunicationSubscription",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
