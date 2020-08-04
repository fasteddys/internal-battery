using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class TrackDBmodelfixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UIX_Tracking_SourceSlug",
                table: "SubscriberEmploymentTypes",
                newName: "UIX_SubscriberEmploymentTypes_Subscriber_EmploymentType");

            migrationBuilder.AlterColumn<string>(
                name: "SourceSlug",
                table: "Tracking",
                type: "VARCHAR(150)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UIX_Tracking_SourceSlug",
                table: "Tracking",
                column: "SourceSlug",
                unique: true,
                filter: "[SourceSlug] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_Tracking_SourceSlug",
                table: "Tracking");

            migrationBuilder.RenameIndex(
                name: "UIX_SubscriberEmploymentTypes_Subscriber_EmploymentType",
                table: "SubscriberEmploymentTypes",
                newName: "UIX_Tracking_SourceSlug");

            migrationBuilder.AlterColumn<string>(
                name: "SourceSlug",
                table: "Tracking",
                type: "VARCHAR(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(150)",
                oldNullable: true);
        }
    }
}
