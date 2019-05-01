using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class tinyidforemailcampaignurl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TinyId",
                table: "CampaignPartnerContact",
                type: "char(8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetedViewName",
                table: "Campaign",
                type: "varchar(100)",
                nullable: true);
            
            migrationBuilder.CreateIndex(
                name: "UIX_PartnerContact_PartnerContactGuid",
                table: "PartnerContact",
                column: "PartnerContactGuid",
                unique: true,
                filter: "[PartnerContactGuid] IS NOT NULL");
            
            migrationBuilder.CreateIndex(
                name: "UIX_CampaignPartnerContact_TinyId",
                table: "CampaignPartnerContact",
                column: "TinyId",
                unique: true,
                filter: "[TinyId] IS NOT NULL");

            migrationBuilder.Sql(@"UPDATE dbo.Campaign SET TargetedViewName = 'Welcome' WHERE Name = 'PPL Lead Gen'");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-04-29 - Bill Koenig - Created
</remarks>
<description>
Returns a single row/column containing a unique identifier. This is a workaround for side-effecting operators within a function.
</description>
<example>
SELECT TOP 1 * FROM [dbo].[v_UniqueIdentifier]
</example>
*/
CREATE VIEW [dbo].[v_UniqueIdentifier]
AS
	SELECT NEWID() [NEWID]
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-04-29 - Bill Koenig - Created
2019-05-01 - Bill Koenig - Removed alphanumeric symbols which share similar physical 
	characteristics to avoid confusion
</remarks>
<description>
Randomly generates an 8 character string using characters A through Z and 0 through 9.
</description>
<example>
SELECT dbo.fn_GenerateTinyId()
</example>
*/
CREATE FUNCTION [dbo].[fn_GenerateTinyId]()
RETURNS CHAR(8)
AS
BEGIN
	DECLARE @s CHAR(8);
	SET @s = (
	SELECT
		c1 AS [text()]
	FROM
		(
		SELECT TOP (8) c1
		FROM
		  (
		VALUES
		  (''A''), (''C''), (''G''), (''H''), (''I''), (''J''),
		  (''K''), (''L''), (''M''), (''N''), (''P''), (''Q''),
          (''R''), (''U''), (''V''), (''W''), (''X''), (''Y''), 
		  (''4''), (''6''), (''9'')	
		  ) AS T1(c1)
		ORDER BY ABS(CHECKSUM((SELECT TOP 1 [NEWID] FROM v_UniqueIdentifier)))
		) AS T2
	FOR XML PATH('''')
	);
	RETURN @s;
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-04-29 - Bill Koenig - Created
2019-05-01 - Bill Koenig - Removed transaction from while loop, SET XACT_ABORT OFF
</remarks>
<description>
Handles the creation of a unique tiny id on insert only. Retries if any collisions are detected up to 3 times.
</description>
<example>
insert into CampaignPartnerContact (CampaignId, PartnerContactId, IsDeleted, CreateDate, CreateGuid, CampaignPartnerContactGuid)
values (22, 100210, 0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', newid())
</example>
*/
CREATE TRIGGER [dbo].[trg_CampaignPartnerContact_INSERT]
ON [dbo].[CampaignPartnerContact] AFTER INSERT 
AS BEGIN

	SET XACT_ABORT OFF
	DECLARE @RetryCount INT
	DECLARE @Success    BIT
	SELECT @RetryCount = 1, @Success = 0
	WHILE @RetryCount <=  3 AND @Success = 0
	BEGIN
		BEGIN TRY
				UPDATE cpc
				SET TinyId = (SELECT dbo.fn_GenerateTinyId())
					, ModifyDate = GETUTCDATE()
					, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
				FROM dbo.CampaignPartnerContact cpc
				JOIN inserted i ON cpc.CampaignPartnerContactGuid = i.CampaignPartnerContactGuid
			SET @Success = 1
		END TRY
		BEGIN CATCH
			IF ERROR_NUMBER() = 2601 -- Violation in unique index
			BEGIN
				SET @RetryCount = @RetryCount + 1   
			END 
			ELSE    
			BEGIN
				THROW;
			END
		END CATCH
	END
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_PartnerContact_PartnerContactGuid",
                table: "PartnerContact");

            migrationBuilder.DropIndex(
                name: "UIX_CampaignPartnerContact_TinyId",
                table: "CampaignPartnerContact");

            migrationBuilder.DropColumn(
                name: "TinyId",
                table: "CampaignPartnerContact");

            migrationBuilder.DropColumn(
                name: "TargetedViewName",
                table: "Campaign");

            migrationBuilder.Sql(@"DROP VIEW [dbo].[v_UniqueIdentifier]");

            migrationBuilder.Sql(@"DROP FUNCTION [dbo].[fn_GenerateTinyId]");

            migrationBuilder.Sql(@"DROP TRIGGER [dbo].[trg_CampaignPartnerContact_INSERT]");
        }
    }
}
