using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_EmailTemplatessproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
</remarks>
<description>
Returns email templates 
</description>
<example>
EXEC [dbo].[System_Get_EmailTemplates] @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_EmailTemplates] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT EmailTemplateId
    	FROM [EmailTemplate]
    	WHERE IsDeleted = 0
    )
    SELECT 
		[SendGridSubAccount],
    	[Name],
    	EmailTemplateGuid, 
		SendGridTemplateId,
    	(SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM [EmailTemplate]
    WHERE IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_EmailTemplates]
            ");

        }
    }
}
