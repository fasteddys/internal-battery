using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingtraitifytocandidate360 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTraitifyAssessmentsVisibleToHiringManagers",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.Sql("UPDATE dbo.Subscriber SET IsTraitifyAssessmentsVisibleToHiringManagers = 1, ModifyDate = GETUTCDATE() WHERE IsDeleted = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTraitifyAssessmentsVisibleToHiringManagers",
                table: "Subscriber");
        }
    }
}
