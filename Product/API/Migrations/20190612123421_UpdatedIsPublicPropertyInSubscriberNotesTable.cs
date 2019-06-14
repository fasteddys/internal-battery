using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class UpdatedIsPublicPropertyInSubscriberNotesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "SubscriberNotes",
                newName: "ViewableByOthersInRecruiterCompany");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ViewableByOthersInRecruiterCompany",
                table: "SubscriberNotes",
                newName: "IsPublic");
        }
    }
}
