using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_SubscriberNotificationssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
 
/*     
<remarks>
2019.12.26 - Jim Brazil - Created 
 
</remarks>
<description>
  Returns subscriber notifications  notifications
</description>
 
*/


CREATE PROCEDURE [dbo].[System_Get_SubscriberNotifications] (
		    @SubscriberGuid UNIQUEIDENTIFIER,
            @Limit int,
            @Offset int,
            @Sort varchar(max),
            @Order varchar(max)
        )
        AS
        BEGIN 
		   SELECT 
				sn.SubscriberNotificationGuid as NotificationGuid,
				n.Title,
				n.[Description],
				CAST(n.IsTargeted AS BIT)  AS  IsTargeted,
				n.ExpirationDate,
				sn.HasRead as HasRead,
				n.CreateDate,
				n.IsDeleted,
				n.ModifyDate,
				n.CreateGuid,
				n.ModifyGuid
			FROM 
				SubscriberNotification sn
				LEFT JOIN Notification n on sn.NotificationId = n.NotificationId
				LEFT JOIN Subscriber s on sn.SubscriberId = s.SubscriberId
			WHERE sn.SubscriberId = s.SubscriberId and sn.IsDeleted = 0	and s.SubscriberGuid =   @SubscriberGuid			
		    ORDER BY  
			CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN n.Title END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN n.CreateDate END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN n.ModifyDate END, 
            CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN n.Title END desc ,
            CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN n.CreateDate END desc ,
			CASE WHEN @Sort = ''descending'' AND @Order = ''recruiter'' THEN n.ModifyDate END desc 
            OFFSET @Offset ROWS
            FETCH FIRST @Limit ROWS ONLY
       END    

  
')
            ");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Get_SubscriberNotifications]
            ");
        }
    }
}
