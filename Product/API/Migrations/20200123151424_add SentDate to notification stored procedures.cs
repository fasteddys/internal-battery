using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addSentDatetonotificationstoredprocedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.26 - Jim Brazil - Created      
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
2020.01.20 - Bill Koenig - Modified to include SentDate
</remarks>
<description>
Returns subscriber notifications  
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotifications] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberNotifications] (
    @SubscriberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT NotificationId
    	FROM SubscriberNotification sn
    	INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
    	WHERE sn.IsDeleted = 0
    	AND s.SubscriberGuid = @SubscriberGuid
    )
    SELECT 
        sn.SubscriberNotificationGuid as NotificationGuid,
        n.Title,
        n.[Description],
        CAST(n.IsTargeted AS BIT)  AS  IsTargeted,
        n.ExpirationDate,
        sn.HasRead as HasRead,
		n.SentDate,
    	(SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM 
        SubscriberNotification sn
        INNER JOIN [Notification] n on sn.NotificationId = n.NotificationId
        INNER JOIN Subscriber s on sn.SubscriberId = s.SubscriberId
    WHERE sn.SubscriberId = s.SubscriberId and sn.IsDeleted = 0	and s.SubscriberGuid = @SubscriberGuid			
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN n.Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN n.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN n.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN n.Title END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN n.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN n.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
2020.01.20 - Jim Brazil - Modified to include SentDate
2020.01.23 - Bill Koenig - Updated previous remark, corrected example
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_Notifications] @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [dbo].[System_Get_Notifications] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT NotificationId
    	FROM [Notification]
    	WHERE IsDeleted = 0
    )
    SELECT 
    	SentDate,
    	NotificationGuid,
    	Title,
    	[Description],
    	CAST(IsTargeted AS BIT)  AS  IsTargeted,
    	ExpirationDate,
    	0 as HasRead,
    	(SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM [Notification]
    WHERE IsDeleted = 0					
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN Title END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
