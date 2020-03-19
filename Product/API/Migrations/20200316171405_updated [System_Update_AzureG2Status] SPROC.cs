using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedSystem_Update_AzureG2StatusSPROC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.09 - JAB - Created 
2020.03.16 - JAB - Modified to accept AzureIndexStatus instead of GuidList and to update statuses from
            status information returned from the azure search indexer

</remarks>
<description>
Performs a bulk update for azure status information for the specfied set of G2 Profiles 
</description>
<example>
DECLARE @ProfileGuidsList AS [dbo].[AzureIndexStatus]
INSERT INTO @ProfileGuidsList VALUES (''39197C5F-5A6B-4107-95C6-0BBA161EE639'',200) 
INSERT INTO @ProfileGuidsList VALUES (''84AE2339-EE3F-414B-BAAD-E0F6C573BD6E'',200) 
INSERT INTO @ProfileGuidsList VALUES (''D4A943FB-2E27-4D50-AEB0-D3A7F06493CB'',200) 
INSERT INTO @ProfileGuidsList VALUES (''3D5D8DB9-D65E-4E41-A589-2F1F0D5D7396'',200)
EXEC [dbo].[System_Update_AzureG2Status] @ProfileGuids = @ProfileGuidsList, @Status = ''Indexed'', @IndexStatusInfo = ''Index on 03/12/2034:12:00''
</example>
*/
ALTER PROCEDURE [G2].[System_Update_AzureG2Status] (
	@ProfileIndexStatuses dbo.AzureIndexStatus READONLY,
    @StatusName Varchar(MAX),
    @IndexStatusInfo VarChar(Max) 
)
AS
BEGIN 
	SET NOCOUNT ON;
	UPDATE G2.Profiles SET
	  AzureIndexStatusId = ( SELECT AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = @StatusName)
	  ,AzureSearchIndexInfo =  @IndexStatusInfo
	  ,ModifyDate = GETUTCDATE()	  
    WHERE ProfileGuid IN (SELECT ProfileGuid FROM @ProfileIndexStatuses WHERE IndexStatus in (200,201) ) ;

    UPDATE G2.Profiles SET	
	   AzureIndexStatusId = ( SELECT AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''Error'')
	  ,AzureSearchIndexInfo =  ''ERROR: '' + (SELECT ErrorMessage From @ProfileIndexStatuses where ProfileGuid = G2.Profiles.ProfileGuid)
	  ,ModifyDate = GETUTCDATE()
      WHERE ProfileGuid IN (SELECT ProfileGuid FROM @ProfileIndexStatuses WHERE IndexStatus not in (200,201) ) 
	  
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.09 - JAB Created 

</remarks>
<description>
Performs a bulk update for azure status information for the specfied set of G2 Profiles 
</description>
<example>
DECLARE @ProfileGuidsList AS [dbo].[GuidList]
INSERT INTO @ProfileGuidsList VALUES (''39197C5F-5A6B-4107-95C6-0BBA161EE639'') 
INSERT INTO @ProfileGuidsList VALUES (''84AE2339-EE3F-414B-BAAD-E0F6C573BD6E'') 
INSERT INTO @ProfileGuidsList VALUES (''D4A943FB-2E27-4D50-AEB0-D3A7F06493CB'') 
INSERT INTO @ProfileGuidsList VALUES (''3D5D8DB9-D65E-4E41-A589-2F1F0D5D7396'')
EXEC [dbo].[System_Update_AzureG2Status] @ProfileGuids = @ProfileGuidsList, @Status = ''Indexed'', @IndexInfo = ''Index on 03/12/2034:12:00''
</example>
*/
CREATE PROCEDURE [g2].[System_Update_AzureG2Status] (
	@ProfileGuids dbo.GuidList READONLY,
    @StatusName Varchar(MAX),
    @IndexStatusInfo VarChar(Max)
)
AS
BEGIN 
	SET NOCOUNT ON;

	UPDATE G2.Profiles SET
	  AzureIndexStatusId = ( SELECT AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = @StatusName)
	  ,AzureSearchIndexInfo =  @IndexStatusInfo
	  ,ModifyDate = GETUTCDATE()
	  
    WHERE ProfileGuid IN (SELECT * from @ProfileGuids) 
END')");

        }
    }
}
