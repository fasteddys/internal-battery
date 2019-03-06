using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class system_delete_campaigncontacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
            EXEC('
/*
<remarks>
2019.03.06 - Jim Brazil - Created
</remarks>
<description>
Handles logical deletion of contats from campaign contacts table
</description>
<example>
DECLARE @ContactGuids AS [dbo].[GuidList]
INSERT INTO @ContactGuids	VALUES (''67510389-8848-4349-9F00-5612AC2AF12C'')
INSERT INTO @ContactGuids	VALUES (''9FCC1E08-1B0D-407F-B0E1-EE835BFFE444'')
EXEC [dbo].[System_Delete_CampaignContacts] @CampaignId = 1, @ContactGuids = @ContactGuids

</example>
*/
CREATE PROCEDURE [dbo].[System_Delete_CampaignContacts] (
	@CampaignId INT,	
	@ContactGuids dbo.GuidList READONLY
)
AS
BEGIN
	BEGIN TRANSACTION;
	BEGIN TRY 		
						     
		/* delete any contacts that are in the ContactGuids list */
		UPDATE dbo.CampaignContact  SET IsDeleted = 1 
		WHERE CampaignContactGuid IN 
		(
		   SELECT CampaignContactGuid FROM CampaignContact cc, Contact c, @ContactGuids cg 
		      WHERE cc.CampaignId = @CampaignId AND cc.ContactId = c.ContactId and c.ContactGuid = cg.Guid		      	   		   
		) 

	END TRY
	BEGIN CATCH
		SELECT 
			 ERROR_NUMBER() AS ErrorNumber
			,ERROR_SEVERITY() AS ErrorSeverity
			,ERROR_STATE() AS ErrorState
			,ERROR_PROCEDURE() AS ErrorProcedure
			,ERROR_LINE() AS ErrorLine
			,ERROR_MESSAGE() AS ErrorMessage;
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
	END CATCH;

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION;
END
                ')
        ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Delete_CampaignContacts]
            ");

        }
    }
}
