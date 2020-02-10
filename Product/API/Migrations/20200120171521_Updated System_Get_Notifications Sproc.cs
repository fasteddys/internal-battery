using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class UpdatedSystem_Get_NotificationsSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
  /*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
2020.01.20 - Mofifed to include SentDate
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_Notifications] @Limit = 2, @Offset = 0, @Sort = ''''title'', @Order = ''ascending''
</example> 
*/
Alter PROCEDURE [dbo].[System_Get_Notifications] (
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
	FROM 
		[Notification]
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
END
            ')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
  /*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_Notifications] @Limit = 2, @Offset = 0, @Sort = ''''title'', @Order = ''ascending''
</example> 
*/
Alter PROCEDURE [dbo].[System_Get_Notifications] (
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
		NotificationGuid,
		Title,
		[Description],
		CAST(IsTargeted AS BIT)  AS  IsTargeted,
		ExpirationDate,
		0 as HasRead,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM 
		[Notification]
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
END
            ')");

        }
    }
}
