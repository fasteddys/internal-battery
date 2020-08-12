using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Update_AzureCandidateStatusSProc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.07.38 - JAB - Created 
 

</remarks>
<description>
Performs a bulk update for azure status information for the specfied set of subscribers
</description>
<example>
DECLARE @ProfileGuidsList AS [dbo].[AzureIndexStatus]
INSERT INTO @ProfileGuidsList VALUES (''39197C5F-5A6B-4107-95C6-0BBA161EE639'',200) 
INSERT INTO @ProfileGuidsList VALUES (''84AE2339-EE3F-414B-BAAD-E0F6C573BD6E'',200) 
INSERT INTO @ProfileGuidsList VALUES (''D4A943FB-2E27-4D50-AEB0-D3A7F06493CB'',200) 
INSERT INTO @ProfileGuidsList VALUES (''3D5D8DB9-D65E-4E41-A589-2F1F0D5D7396'',200)
EXEC [dbo].[System_Update_AzureCandidateStatus] @ProfileGuids = @ProfileGuidsList, @Status = ''Indexed'', @IndexStatusInfo = ''Index on 03/12/2034:12:00''
</example>
*/
CREATE PROCEDURE [B2B].[System_Update_AzureCandidateStatus] (
	@ProfileIndexStatuses dbo.AzureIndexStatus READONLY,
    @StatusName Varchar(MAX),
    @IndexStatusInfo VarChar(Max) 
)
AS
BEGIN 
	SET NOCOUNT ON;
	UPDATE dbo.Subscriber SET
	  AzureIndexStatusId = ( SELECT AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = @StatusName)
	  ,AzureSearchIndexInfo =  @IndexStatusInfo
	  ,AzureIndexModifyDate = GETUTCDATE()	  
    WHERE SubscriberGuid IN (SELECT ProfileGuid FROM @ProfileIndexStatuses WHERE IndexStatus in (200,201) ) ;

    UPDATE dbo.Subscriber SET	
	   AzureIndexStatusId = ( SELECT AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''Error'')
	  ,AzureSearchIndexInfo =  ''ERROR: '' + (SELECT ErrorMessage From @ProfileIndexStatuses where ProfileGuid = dbo.Subscriber.SubscriberGuid)
	  ,AzureIndexModifyDate = GETUTCDATE()
      WHERE SubscriberGuid IN (SELECT ProfileGuid FROM @ProfileIndexStatuses WHERE IndexStatus not in (200,201) ) 
	  
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE  [B2B].[System_Update_AzureCandidateStatus]
            ");
        }
    }
}
