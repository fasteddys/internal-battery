using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addIsVerifiedtoSkill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Skill",
                nullable: true,
                defaultValueSql: "1");
            
            migrationBuilder.Sql("UPDATE dbo.Skill SET IsVerified = (CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END), ModifyDate = GETUTCDATE()");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.06.26 - Bill Koenig - Created
</remarks>
<description>
A simple user-defined table type that supports a list of strings whose limit is 450 characters.
</description>
*/
CREATE TYPE [dbo].[StringList] AS TABLE ([string] NVARCHAR(450))')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.06.26 - Bill Koenig - Created
</remarks>
<description>
Handles updates to a candidate''s skills. If a skill does not exist, it will be added to our system as not verified.
</description>
<example>
DECLARE @SkillNames AS [dbo].[StringList]
INSERT INTO @SkillNames VALUES (''c#'') 
INSERT INTO @SkillNames VALUES (''mvc'') 
INSERT INTO @SkillNames VALUES (''sql server'') 
INSERT INTO @SkillNames VALUES (''pizza sauce'') 
INSERT INTO @SkillNames VALUES (''videoconferencing software'')
EXEC [dbo].[System_Update_CandidateSkills] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @SkillNames = @SkillNames
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_CandidateSkills] (
	@SubscriberGuid UNIQUEIDENTIFIER,	
	@SkillNames dbo.StringList READONLY
)
AS
BEGIN
	BEGIN TRANSACTION;
	BEGIN TRY
		-- need this for skill modifications (when no skills exist for a subscriber, cannot use joins based on SubscriberGuid)
		DECLARE @SubscriberId INT = (SELECT TOP 1 s.SubscriberId FROM Subscriber s WHERE s.SubscriberGuid = @SubscriberGuid)

		-- replace these with meaningful values (once we have a mechanism in place to generate them)
		DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
		DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
		
		-- create a local temp table so that we can modify the contents of the read-only user-defined table type
		CREATE TABLE #SkillNames (SkillName NVARCHAR(450) NOT NULL)
		
		-- trim leading and trailing spaces from the skill names
		INSERT INTO #SkillNames (SkillName)
		SELECT TRIM(string)
		FROM @SkillNames
		
		-- create skills that do not exist as non-verified
		INSERT INTO dbo.Skill (IsDeleted, CreateDate, CreateGuid, SkillGuid, SkillName, IsVerified)
		SELECT 0, GETUTCDATE(), @CreateGuid, NEWID(), sn.SkillName, 0
		FROM #SkillNames sn
		LEFT JOIN dbo.Skill s ON sn.SkillName = s.SkillName
		WHERE s.SkillId IS NULL 
		
		-- update skills which were previously deleted to be active again but non-verified
		UPDATE s
		SET s.ModifyDate = GETUTCDATE()
			, ModifyGuid = @ModifyGuid
			, IsDeleted = 0
			, IsVerified = 0
		FROM dbo.Skill s
		INNER JOIN #SkillNames sn ON sn.SkillName = s.SkillName
		WHERE s.IsDeleted = 1 

		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillName
			FROM SubscriberSkill ss 
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			WHERE p.SubscriberGuid = @SubscriberGuid
		)
		INSERT INTO dbo.SubscriberSkill (IsDeleted, CreateDate, CreateGuid, SubscriberId, SkillId, SubscriberSkillGuid)
		SELECT 0, GETUTCDATE(), @CreateGuid, @SubscriberId, s.SkillId, NEWID()
		FROM #SkillNames sn
		INNER JOIN Skill s ON sn.SkillName = s.SkillName
		LEFT JOIN existingSkills es ON sn.SkillName = es.SkillName
		WHERE es.SkillId IS NULL

		-- saved skills which were previously deleted should be made active
		UPDATE 
			ss
		SET
			ss.IsDeleted = 0
			, ss.ModifyDate = GETUTCDATE()
			, ss.ModifyGuid = @ModifyGuid
		FROM  
			SubscriberSkill ss
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			INNER JOIN #SkillNames sn ON s.SkillName = sn.SkillName
		WHERE
			p.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 1

		-- existing skills not specified in saved skills should be logically deleted
		UPDATE 
			ss
		SET
			ss.IsDeleted = 1
			, ss.ModifyDate = GETUTCDATE()
			, ss.ModifyGuid = @ModifyGuid
		FROM  
			SubscriberSkill ss
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			LEFT JOIN #SkillNames sn ON s.SkillName = sn.SkillName
		WHERE
			sn.SkillName IS NULL
			AND p.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0
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
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.06.26 - Bill Koenig - Created
</remarks>
<description>
Returns candidate skills. Includes unverified skills (if any exist).
</description>
<example>
EXEC [dbo].[System_Get_CandidateSkills] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 1, @Offset = 0, @Sort = ''name'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_CandidateSkills] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN
    WITH allRecords AS (
        SELECT k.SkillGuid
			, k.SkillName [Name]
			, ss.CreateDate
			, ss.ModifyDate
        FROM dbo.Subscriber s
		INNER JOIN dbo.SubscriberSkill ss ON s.SubscriberId = ss.SubscriberId
		INNER JOIN dbo.Skill k ON ss.SkillId = k.SkillId
		WHERE k.IsDeleted = 0
		AND ss.IsDeleted = 0
		AND s.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid)
    SELECT SkillGuid
		, [Name]
        , (SELECT COUNT(1) FROM allRecords) AS TotalRecords
    FROM allRecords
    ORDER BY  
        CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
        CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
        CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC ,        
        CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
        CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Skill");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Update_CandidateSkills]");

            migrationBuilder.Sql("DROP TYPE [dbo].[StringList]");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_CandidateSkills]");
        }
    }
}