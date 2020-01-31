using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Update_NotificationGroupssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.12.18 - JAB - Created

</remarks>
<description>
Update the groups associated with a notifications 
</description>
<example>
 
</example>
*/
Create PROCEDURE [dbo].[System_Update_NotificationGroups] (
    @SubscriberGuid UniqueIdentifier, 
    @NotificationGuid UniqueIdentifier, 
	@GroupGuids dbo.GuidList  READONLY
    
)
AS
BEGIN 

    DECLARE @NotificationId Int;	
	SELECT @NotificationId = NotificationId FROM Notification WHERE NotificationGuid = @NotificationGuid;

    /* Delete existing groups that are not in the guid list */
	UPDATE NotificationGroup Set 
		ISDeleted = 1 
	WHERE 
	    NotificationId = @NotificationId AND 
		GroupId Not IN
		( 
		   SELECT GroupID FROM [Group] g 
		   JOIN @GroupGuids gg on g.GroupGuid = gg.Guid
		)
						 
	/* Mark all existing groups that are not in the guid list as not deleted  */
	UPDATE NotificationGroup Set 
		ISDeleted = 0
	WHERE 
	    NotificationId = @NotificationId AND 
		GroupId IN 
		( 
		   SELECT GroupID FROM [Group] g 
		   JOIN @GroupGuids gg on g.GroupGuid = gg.Guid
		)
   
    /* Insert new Groups into Notification Group */
    INSERT INTO NotificationGroup 
	( 
	   IsDeleted,
	   CreateDate,
	   CreateGuid,
	   NotificationId,
	   GroupId 
	 ) 
	 SELECT  
	   0,
	   GETUTCDATE(),
	   @SubscriberGuid,
	   @NotificationId,
	   g.GroupId
	 FROM 
	    @GroupGuids gg
	    JOIN [Group] g on gg.Guid = g.GroupGuid and g.IsDeleted = 0
     WHERE 
	    g.GroupId NOT IN ( SELECT GroupId FROM NotificationGroup WHERE NotificationId = @NotificationId )
 
END
')");





            migrationBuilder.CreateIndex(
                name: "UIX_NotificationGroup_Group",
                table: "NotificationGroup",
                columns: new[] { "NotificationGroupId", "GroupId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Update_NotificationGroup]
            ");



            migrationBuilder.DropIndex(
                name: "UIX_NotificationGroup_Group",
                table: "NotificationGroup");
        }
    }
}
