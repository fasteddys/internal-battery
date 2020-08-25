using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class GetCrossChqStatusByResume : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.08.26 - Joey Herrington - Created
</remarks>
<description>
Get''s a list of CrossChq statuses by resume upload date
</description>
<example>
DECLARE @totalRows INT;
EXEC [dbo].[System_Get_CrossChqByResumeUploadDate] @startDate = ''2020-08-20'', @limit = 10, @offset = 0, @sort = ''ascending'', @orderBy = ''CreateDate'', @rowCount = @totalRows OUTPUT
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_CrossChqByResumeUploadDate] (
	@startDate DATETIME,
	@limit INT,
	@offset INT,
	@sort NVARCHAR(),
	@orderBy NVARCHAR(),
	@rowCount INT OUTPUT
)
AS
BEGIN




	-- TODO:  Implement SQL




END')");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_Get_CrossChqByResumeUploadDate]");
        }
    }
}
