using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class fixorphanactivejobpostings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE p
SET p.ModifyDate = GETUTCDATE()
	, p.ModifyGuid = '00000000-0000-0000-0000-000000000000'
	, p.JobPostingId = NULL
	, p.JobPageStatusId = 4
FROM JobPage p
INNER JOIN JobPosting j ON p.JobPostingId = j.JobPostingId
WHERE j.IsDeleted = 1
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
