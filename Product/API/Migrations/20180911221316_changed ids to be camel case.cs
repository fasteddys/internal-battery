using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class changedidstobecamelcase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorTermsOfServiceID",
                table: "VendorTermsOfService",
                newName: "VendorTermsOfServiceId");

            migrationBuilder.RenameColumn(
                name: "TagID",
                table: "Tag",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "NewsID",
                table: "News",
                newName: "NewsId");

            migrationBuilder.RenameColumn(
                name: "GenderID",
                table: "Gender",
                newName: "GenderId");

            migrationBuilder.RenameColumn(
                name: "BadgeEarnedID",
                table: "BadgeEarned",
                newName: "BadgeEarnedId");

            migrationBuilder.RenameColumn(
                name: "BadgeCourseID",
                table: "BadgeCourse",
                newName: "BadgeCourseId");

            migrationBuilder.RenameColumn(
                name: "BadgeID",
                table: "Badge",
                newName: "BadgeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorTermsOfServiceId",
                table: "VendorTermsOfService",
                newName: "VendorTermsOfServiceID");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "Tag",
                newName: "TagID");

            migrationBuilder.RenameColumn(
                name: "NewsId",
                table: "News",
                newName: "NewsID");

            migrationBuilder.RenameColumn(
                name: "GenderId",
                table: "Gender",
                newName: "GenderID");

            migrationBuilder.RenameColumn(
                name: "BadgeEarnedId",
                table: "BadgeEarned",
                newName: "BadgeEarnedID");

            migrationBuilder.RenameColumn(
                name: "BadgeCourseId",
                table: "BadgeCourse",
                newName: "BadgeCourseID");

            migrationBuilder.RenameColumn(
                name: "BadgeId",
                table: "Badge",
                newName: "BadgeID");
        }
    }
}
