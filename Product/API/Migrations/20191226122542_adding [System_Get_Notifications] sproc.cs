using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_Notificationssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
 
  /*     
<remarks>
2019.12.23 - Jim Brazil - Created 
 
</remarks>
<description>
  Returns system notifications
</description>
 
*/


create PROCEDURE [dbo].[System_Get_Notifications] (
 
            @Limit int,
            @Offset int,
            @Sort varchar(max),
            @Order varchar(max)
        )
        AS
        BEGIN 
		   SELECT 
				NotificationGuid,
				Title,
				[Description],
				CAST(IsTargeted AS BIT)  AS  IsTargeted,
				ExpirationDate,
				0 as HasRead,
				CreateDate,
				IsDeleted,
				ModifyDate,
				CreateGuid,
				ModifyGuid
			FROM 
				Notification
			WHERE IsDeleted = 0					
		    ORDER BY  
			CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN Title END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
            CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN Title END desc ,
            CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN CreateDate END desc ,
			CASE WHEN @Sort = ''descending'' AND @Order = ''recruiter'' THEN ModifyDate END desc 
            OFFSET @Offset ROWS
            FETCH FIRST @Limit ROWS ONLY
       END    

')
            ");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_Notifications]
            ");


        }
    }
}
