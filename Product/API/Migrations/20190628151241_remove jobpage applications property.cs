using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removejobpageapplicationsproperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.Sql(@"UPDATE dbo.JobPage SET ModifyDate = GETUTCDATE(), RawData = JSON_MODIFY(RawData, '$.applications', NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no rollback for this operation
        }
    }
}
