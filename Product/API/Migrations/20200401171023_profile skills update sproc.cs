using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class profileskillsupdatesproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.04.01 - Bill Koenig - Created
</remarks>
<description>
Handles updates (add and delete) for profile skills
</description>
<example>
SET NOCOUNT ON
DECLARE @ValidationErrors NVARCHAR(MAX)
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids
VALUES (''3B0BE2F1-3A52-4671-B9AB-54C42507B4EA'') -- existing skill
INSERT INTO @SkillGuids
VALUES (''155FA15C-CA6D-4018-80BB-930C5099F3C9'') -- new skill
INSERT INTO @SkillGuids
VALUES (''FC296C51-CDAD-4BC4-8986-E289FA969C44'') -- previously deleted skill
EXEC [G2].[System_Update_ProfileSkillsForRecruiter] 
	@RecruiterSubscriberGuid = ''4541A9C1-C464-4354-BE24-4A9650A9AF6D'',
	@ProfileGuid = ''32157BEC-F382-4F06-AF34-C52E35EA7939'', 
	@SkillGuids = @SkillGuids,
	@ValidationErrors = @ValidationErrors OUTPUT
PRINT @ValidationErrors
</example>
*/
CREATE PROCEDURE [G2].[System_Update_ProfileSkillsForRecruiter] (
    @RecruiterSubscriberGuid UNIQUEIDENTIFIER,	
	@ProfileGuid UNIQUEIDENTIFIER, 
	@SkillGuids dbo.GuidList READONLY,
	@ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
	-- replace these with meaningful values (once we have a mechanism in place to generate them)
	DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
	DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''

    -- need to set this to be an empty string otherwise concatenation won''t work
    SET @ValidationErrors = ''''

    IF ((SELECT COUNT(1) FROM G2.Profiles WHERE ProfileGuid = @ProfileGuid AND IsDeleted = 0) <> 1)
    	SET @ValidationErrors = @ValidationErrors + ''A unique profile was not found for the provided identifier;''
		
	IF NOT EXISTS (
		SELECT p.ProfileId
		FROM G2.Profiles p
		INNER JOIN Company c ON p.CompanyId = c.CompanyId
		INNER JOIN RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
		WHERE p.ProfileGuid = @ProfileGuid
		AND s.SubscriberGuid = @RecruiterSubscriberGuid
		AND s.IsDeleted = 0
		AND p.IsDeleted = 0)
		SET @ValidationErrors = @ValidationErrors + ''The specified recruiter does not have permission to edit this profile;''

	IF EXISTS(
		SELECT sg.[Guid]
		FROM @SkillGuids sg 
		LEFT JOIN dbo.Skill s ON s.SkillGuid = sg.[Guid]
		WHERE s.SkillGuid IS NULL)
	BEGIN
		SET @ValidationErrors = @ValidationErrors + ''Invalid skill(s) specified;''
	END

    -- perform operations if there were no validation errors
    IF(LEN(@ValidationErrors) = 0)
    BEGIN	
		-- need this for skill modifications (when no skills exist for a profile, cannot use joins based on ProfileGuid)
		DECLARE @ProfileId INT = (SELECT TOP 1 p.ProfileId FROM G2.Profiles p WHERE p.ProfileGuid = @ProfileGuid)
		
		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillGuid
			FROM G2.ProfileSkills ps 
			INNER JOIN G2.Profiles p ON ps.ProfileId = p.ProfileId
			INNER JOIN Skill s ON ps.SkillId = s.SkillId
			WHERE p.ProfileGuid = @ProfileGuid
		)
		INSERT INTO G2.ProfileSkills(IsDeleted, CreateDate, CreateGuid, ProfileId, SkillId, ProfileSkillGuid)
		SELECT 0, GETUTCDATE(), @CreateGuid, @ProfileId, s.SkillId, NEWID()
		FROM @SkillGuids sg
		INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		WHERE es.SkillGuid IS NULL

		-- saved skills which were previously deleted should be made active
		UPDATE 
			ps
		SET
			ps.IsDeleted = 0
			, ps.ModifyDate = GETUTCDATE()
			, ps.ModifyGuid = @ModifyGuid
		FROM  
			G2.ProfileSkills ps
			INNER JOIN G2.Profiles p ON ps.ProfileId = p.ProfileId
			INNER JOIN Skill s ON ps.SkillId = s.SkillId
			INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			p.ProfileGuid = @ProfileGuid
			AND ps.IsDeleted = 1

		-- existing skills not specified in saved skills should be logically deleted
		UPDATE 
			ps
		SET
			ps.IsDeleted = 1
			, ps.ModifyDate = GETUTCDATE()
			, ps.ModifyGuid = @ModifyGuid
		FROM  
			G2.ProfileSkills ps
			INNER JOIN G2.Profiles p ON ps.ProfileId = p.ProfileId
			INNER JOIN Skill s ON ps.SkillId = s.SkillId
			LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			sg.[Guid] IS NULL
			AND p.ProfileGuid = @ProfileGuid
			AND ps.IsDeleted = 0
	END	    
	ELSE
    BEGIN
    	-- remove the last occurrence of the semicolon
    	SET @ValidationErrors = CASE WHEN CHARINDEX('';'', @ValidationErrors) = 0 THEN @ValidationErrors ELSE LEFT(@ValidationErrors, LEN(@ValidationErrors) - CHARINDEX('';'', REVERSE(@ValidationErrors) + '';'')) END
    END
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [G2].[System_Update_ProfileSkillsForRecruiter]");
        }
    }
}
