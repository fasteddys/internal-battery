using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobpostingskillupdatebyguid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.15 - Bill Koenig - Created
</remarks>
<description>
Handles add and update operations to job posting skills.
</description>
<example>
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids
VALUES (''E89C5C06-EE5F-49D0-AB52-BEC5905956C8'') -- existing skill
INSERT INTO @SkillGuids
VALUES (''C4933CE9-BACF-4F2A-8247-5D2393B134CD'') -- new skill
INSERT INTO @SkillGuids
VALUES (''78F7D321-FDDE-46D2-BAA2-5F805EF57F5C'') -- previously deleted skill
EXEC [dbo].[System_Update_JobPostingSkillsByGuid] @JobPostingGuid = ''A1621DFF-8CE8-45B1-8CCD-520FD5C7132F'', @SkillGuids = @SkillGuids
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_JobPostingSkillsByGuid] (
	@JobPostingGuid UNIQUEIDENTIFIER,
	@SkillGuids dbo.GuidList READONLY
)
AS
BEGIN

	BEGIN TRANSACTION;
	BEGIN TRY
		
		-- replace these with meaningful values (once we have a mechanism in place to generate them)
		DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
		DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''

		-- get the job posting id 
		DECLARE @JobPostingId INT = (SELECT TOP 1 JobPostingId FROM dbo.JobPosting WHERE JobPostingGuid = @JobPostingGuid)

		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillGuid
			FROM dbo.JobPostingSkill jps 			
			INNER JOIN Skill s ON jps.SkillId = s.SkillId
			WHERE jps.JobPostingId= @JobPostingId
		)
		INSERT INTO dbo.JobPostingSkill(IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, JobPostingId, SkillId, JobPostingSkillGuid)
		SELECT 0, GETUTCDATE(), GETUTCDATE(), @CreateGuid, @ModifyGuid, @JobPostingId, s.SkillId, NEWID()
		FROM @SkillGuids sg
		INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		WHERE es.SkillGuid IS NULL

		-- saved skills which were previously deleted should be made active
		UPDATE 
			jps
		SET
			jps.IsDeleted = 0
			, jps.ModifyDate = GETUTCDATE()
			, jps.ModifyGuid = @ModifyGuid
		FROM  
			JobPostingSkill jps			
			INNER JOIN Skill s ON jps.SkillId = s.SkillId
			INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			jps.JobPostingId = @JobPostingId
			AND jps.IsDeleted = 1

		-- existing skills not specified in saved skills should be logically deleted
		UPDATE 
			jps
		SET
			jps.IsDeleted = 1
			, jps.ModifyDate = GETUTCDATE()
			, jps.ModifyGuid = @ModifyGuid
		FROM  
			JobPostingSkill jps			
			INNER JOIN Skill s ON jps.SkillId = s.SkillId
			LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			sg.[Guid] IS NULL
			AND jps.JobPostingId = @JobPostingId
			AND jps.IsDeleted = 0
 
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
