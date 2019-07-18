using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscribernotificationemails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationEmailsEnabled",
                table: "Subscriber",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.07.12 - Bill Koenig - Created
</remarks>
<description>
Retrieves the most recent unread notification for each subscriber along with a count of the total number of unread notifications. 
The result set is further limited to notifications that were created X number of days ago (ReminderLookbackInDays). The number of
unread notifications is not affected by the ReminderLookbackInDays parameter. Expired notifications are not counted and subscribers
that have disabled notifications are excluded from the result set.
</description>
<example>
EXEC [dbo].[System_Get_UnreadSubscriberNotifications] @ReminderLookbackInDays = 1
EXEC [dbo].[System_Get_UnreadSubscriberNotifications] @ReminderLookbackInDays = 7
EXEC [dbo].[System_Get_UnreadSubscriberNotifications] @ReminderLookbackInDays = 30
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_UnreadSubscriberNotifications] (
	@ReminderLookbackInDays INT
)
AS
BEGIN
	
	DECLARE @CurrentDate DATE = CONVERT(DATE, GETUTCDATE())		
		
	;WITH unreadNotifications AS (
		SELECT s.Email, COUNT(1) [TotalUnread]
		FROM SubscriberNotification sn WITH(NOLOCK)
		INNER JOIN Subscriber s WITH(NOLOCK) ON sn.SubscriberId = s.SubscriberId
		INNER JOIN [Notification] n WITH(NOLOCK) ON sn.NotificationId = n.NotificationId
		WHERE (n.ExpirationDate IS NULL OR n.ExpirationDate > @CurrentDate)
		AND sn.HasRead = 0
		AND s.NotificationEmailsEnabled = 1
		AND sn.IsDeleted = 0
		AND n.IsDeleted = 0
		AND s.IsDeleted = 0
		GROUP BY s.Email)
	, mostRecentUnreadNotification AS (
		SELECT s.SubscriberGuid, s.Email, s.FirstName, n.Title, ROW_NUMBER() OVER (PARTITION BY s.Email ORDER BY sn.CreateDate DESC) [MostRecentDesc]
		FROM SubscriberNotification sn WITH(NOLOCK)
		INNER JOIN Subscriber s WITH(NOLOCK) ON sn.SubscriberId = s.SubscriberId
		INNER JOIN [Notification] n WITH(NOLOCK) ON sn.NotificationId = n.NotificationId
		WHERE DATEDIFF(DAY, CONVERT(DATE, sn.CreateDate), @CurrentDate) = @ReminderLookbackInDays
		AND (n.ExpirationDate IS NULL OR n.ExpirationDate > @CurrentDate)
		AND sn.HasRead = 0
		AND s.NotificationEmailsEnabled = 1
		AND sn.IsDeleted = 0
		AND n.IsDeleted = 0
		AND s.IsDeleted = 0
	)
	SELECT mrun.SubscriberGuid, mrun.Email, mrun.FirstName, mrun.Title, un.TotalUnread
	FROM mostRecentUnreadNotification mrun
	INNER JOIN unreadNotifications un ON mrun.Email = un.Email
	WHERE mrun.MostRecentDesc = 1

END 
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationEmailsEnabled",
                table: "Subscriber");

            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_Get_UnreadSubscriberNotifications]");
        }
    }
}
