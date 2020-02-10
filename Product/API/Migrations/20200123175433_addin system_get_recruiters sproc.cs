using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addinsystem_get_recruiterssproc : Migration
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
    EXEC [dbo].[System_Get_Recruiters] @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
    </example> 
    */
    CREATE PROCEDURE [dbo].[System_Get_Recruiters] (
        @Limit int,
        @Offset int,
        @Sort varchar(max),
        @Order varchar(max)
    )
    AS
    BEGIN 
    	WITH allRecords AS (
    		SELECT RecruiterId
    		FROM Recruiter 
    		WHERE IsDeleted = 0
    	)
        SELECT  
			r.RecruiterGuid,
			s.SubscriberGuid,
			c.CompanyGuid,
			r.FirstName,
			r.LastName,
			r.Email,
			r.PhoneNumber,
			r.CreateDate,
			r.ModifyDate,
			null as IsInAuth0RecruiterGroupRecruiter, 
    	   (SELECT COUNT(1) FROM allRecords) [TotalRecords]
        FROM Recruiter r 
		LEFT JOIN Company c on c.CompanyId = r.CompanyId
		LEFT JOIN Subscriber s on r.SubscriberId = s.SubscriberId
		 
        WHERE  r.IsDeleted = 0
        ORDER BY  
        CASE WHEN @Order = ''ascending'' AND @Sort = ''firstName'' THEN r.FirstName END,
		CASE WHEN @Order = ''ascending'' AND @Sort = ''lastName'' THEN r.LastName END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN r.CreateDate END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN r.ModifyDate END, 
		CASE WHEN @Order = ''ascending'' AND @Sort = ''email'' THEN r.Email END, 
        CASE WHEN @Order = ''descending'' AND @Sort = ''firstName'' THEN r.FirstName END DESC ,
		CASE WHEN @Order = ''descending'' AND @Sort = ''lastName'' THEN r.LastName END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN r.CreateDate END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN r.ModifyDate END DESC ,
		CASE WHEN @Order = ''descending'' AND @Sort = ''email'' THEN r.Email END DESC 
        OFFSET @Offset ROWS
        FETCH FIRST @Limit ROWS ONLY
    END
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop procedure [dbo].[System_Get_Recruiters]");
        }
    }
}
