using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class entitytypeadjustments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction");

            migrationBuilder.AlterColumn<int>(
                name: "EntityTypeId",
                table: "SubscriberAction",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EntityType",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction",
                column: "EntityTypeId",
                principalTable: "EntityType",
                principalColumn: "EntityTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction");

            migrationBuilder.AlterColumn<int>(
                name: "EntityTypeId",
                table: "SubscriberAction",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EntityType",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction",
                column: "EntityTypeId",
                principalTable: "EntityType",
                principalColumn: "EntityTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
