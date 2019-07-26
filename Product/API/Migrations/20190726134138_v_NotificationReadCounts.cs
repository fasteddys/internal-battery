using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class v_NotificationReadCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-07-26 - Bill Koenig - Created
</remarks>
<description>
Returns subscriber notification read counts grouped by notification title and create date
</description>
<example>
SELECT * FROM [dbo].[v_NotificationReadCounts]
</example>
*/
CREATE VIEW [dbo].[v_NotificationReadCounts] 
AS 

	SELECT n.Title [NotificationTitle], n.CreateDate [PublishedDate], SUM(CASE WHEN sn.HasRead = 1 THEN 1 ELSE 0 END) [ReadCount]
	FROM [Notification] n
	LEFT JOIN SubscriberNotification sn ON n.NotificationId = sn.NotificationId
	LEFT JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
    WHERE n.IsDeleted = 0
	GROUP BY n.Title, n.CreateDate
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW dbo.v_NotificationReadCounts");
        }
    }
}
