using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Partners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql(@"EXEC('/*     
<remarks>
    2020.01.21 - Jyoti Guin - Created
    </remarks>
    <description>
    Retrieves Partners
    </description>
    <example>
    EXEC [dbo].[System_Get_Partners] @Limit = 10, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
    </example>
    */
    CREATE PROCEDURE [dbo].[System_Get_Partners] (
        @Limit int,
        @Offset int,
        @Sort varchar(max),
        @Order varchar(max)
    )
    AS
    BEGIN 
    	WITH allRecords AS (
    		SELECT PartnerId
    		FROM Partner 		
    		WHERE IsDeleted = 0
    	)
        SELECT PartnerGuid
            , Name
    		, Description   
    		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
        FROM Partner
        WHERE IsDeleted = 0
        ORDER BY 
        CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN Name END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''description'' THEN Description END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN Name  END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''description'' THEN Description END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
        OFFSET @Offset ROWS
        FETCH FIRST @Limit ROWS ONLY
    END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_Get_Partners]");
        }
    }
}
