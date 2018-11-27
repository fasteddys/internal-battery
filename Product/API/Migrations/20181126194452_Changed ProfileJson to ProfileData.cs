using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ChangedProfileJsontoProfileData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileJson",
                table: "SubscriberProfileStagingStore",
                newName: "ProfileData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileData",
                table: "SubscriberProfileStagingStore",
                newName: "ProfileJson");
        }
    }
}
