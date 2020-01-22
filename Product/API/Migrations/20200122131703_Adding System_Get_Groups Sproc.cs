using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Get_GroupsSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
 /*     
    <remarks>     
    2020.01.22 - JAB - Created
    </remarks>
    <description>
    Returns Groups
    </description>
    <example>
    EXEC [dbo].[System_Get_Groups] @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
    </example> 
    */
    CREATE PROCEDURE [dbo].[System_Get_Groups] (
        @Limit int,
        @Offset int,
        @Sort varchar(max),
        @Order varchar(max)
    )
    AS
    BEGIN 
    	WITH allRecords AS (
    		SELECT GroupId
    		FROM [Group] 
    		WHERE IsDeleted = 0
    	)
        SELECT  
			GroupGUid,
			[Name],
			[Description],
			Isleavable,
			[Path], 
    	   (SELECT COUNT(1) FROM allRecords) [TotalRecords]
        FROM [Group] 
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
    END
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop procedure [dbo].[System_Get_Groups]");
        }
    }
}
