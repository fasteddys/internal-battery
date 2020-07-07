using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class remove0milesfromcommutedistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.CommuteDistance SET IsDeleted = 1, ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE DistanceRange = '0 miles'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
