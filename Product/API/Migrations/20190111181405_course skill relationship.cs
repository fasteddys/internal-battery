using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class courseskillrelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseSkill",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    SkillId = table.Column<int>(nullable: false),
                    CourseSkillGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSkill", x => new { x.CourseId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_CourseSkill_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSkill_SkillId",
                table: "CourseSkill",
                column: "SkillId");

            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.01.11 - Bill Koenig - Created
</remarks>
<description>
Handles updates to an entity''s related skills.
</description>
<example>
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids	VALUES (''67510389-8848-4349-9F00-5612AC2AF12C'')
INSERT INTO @SkillGuids	VALUES (''9FCC1E08-1B0D-407F-B0E1-EE835BFFE444'')
EXEC [dbo].[System_Update_EntitySkills] @EntityGuid = ''C01AD389-1824-481E-BB7D-1BEC8F80D085'', @EntityType = ''Course'', @SkillGuids = @SkillGuids
EXEC [dbo].[System_Update_EntitySkills] @EntityGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9'', @EntityType = ''Subscriber'', @SkillGuids = @SkillGuids
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_EntitySkills] (
	@EntityGuid UNIQUEIDENTIFIER,
	@EntityType NVARCHAR(100),
	@SkillGuids dbo.GuidList READONLY
)
AS
BEGIN

	BEGIN TRANSACTION;
	BEGIN TRY

		-- retrieve the primary key for the entity whose skills we want to modify
		DECLARE @EntityId INT
		DECLARE @SqlString NVARCHAR(MAX) = N''SELECT @EntityId = MAX(e.'' + @EntityType + N''Id) FROM dbo.'' + @EntityType + N'' e WHERE e.'' + @EntityType + N''Guid = @EntityGuid''
		DECLARE @ParameterDefinition NVARCHAR(500) = N''@EntityGuid UNIQUEIDENTIFIER, @EntityId INT OUTPUT''
		EXECUTE sp_executesql @SqlString, @ParameterDefinition, @EntityGuid = @EntityGuid, @EntityId = @EntityId OUTPUT
		
		-- only continue if the entity is valid
		IF(@EntityId > 0)
		BEGIN
		    -- replace these with meaningful values (once we have a mechanism in place to generate them)
		    DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
		    DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''

		    -- saved skills which do not exist should be created
		    SET @SqlString = N''
		    ;WITH existingSkills AS (
			    SELECT s.SkillId, s.SkillGuid
			    FROM dbo.'' + @EntityType + N''Skill es 
			    INNER JOIN dbo.'' + @EntityType + N'' e ON es.'' + @EntityType + N''Id = e.'' + @EntityType + N''Id
			    INNER JOIN Skill s ON es.SkillId = s.SkillId
			    WHERE e.'' + @EntityType + N''Id = @EntityId
		    )
		    INSERT INTO dbo.'' + @EntityType + N''Skill (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, '' + @EntityType + N''Id, SkillId, '' + @EntityType + N''SkillGuid)
		    SELECT 0, GETUTCDATE(), GETUTCDATE(), @CreateGuid, @ModifyGuid, @EntityId, s.SkillId, NEWID()
		    FROM @SkillGuids sg
		    INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		    LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		    WHERE es.SkillGuid IS NULL''
		    SET @ParameterDefinition = N''@CreateGuid UNIQUEIDENTIFIER, @ModifyGuid UNIQUEIDENTIFIER, @EntityId INT, @SkillGuids dbo.GuidList readonly''
		    EXECUTE sp_executesql @SqlString, @ParameterDefinition, @CreateGuid = @CreateGuid, @ModifyGuid = @ModifyGuid, @EntityId = @EntityId, @SkillGuids = @SkillGuids

		    -- saved skills which were previously deleted should be made active
		    SET @SqlString = N''
		    UPDATE 
			    es
		    SET
			    es.IsDeleted = 0
			    , es.ModifyDate = GETUTCDATE()
			    , es.ModifyGuid = @ModifyGuid
		    FROM  
			    dbo.'' + @EntityType + N''Skill es
			    INNER JOIN dbo.'' + @EntityType + N'' e ON es.'' + @EntityType + N''Id = e.'' + @EntityType + N''Id
			    INNER JOIN Skill s ON es.SkillId = s.SkillId
			    INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		    WHERE
			    e.'' + @EntityType + N''Id = @EntityId
			    AND es.IsDeleted = 1''
		    SET @ParameterDefinition = N''@ModifyGuid UNIQUEIDENTIFIER, @EntityId INT, @SkillGuids dbo.GuidList readonly''
		    EXECUTE sp_executesql @SqlString, @ParameterDefinition, @ModifyGuid = @ModifyGuid, @EntityId = @EntityId, @SkillGuids = @SkillGuids

		    -- existing skills not specified in saved skills should be logically deleted
		    SET @SqlString = N''
		    UPDATE 
			    es
		    SET
			    es.IsDeleted = 1
			    , es.ModifyDate = GETUTCDATE()
			    , es.ModifyGuid = @ModifyGuid
		    FROM  
			    dbo.'' + @EntityType + N''Skill es
			    INNER JOIN dbo.'' + @EntityType + N'' e ON es.''+ @EntityType + N''Id = e.'' + @EntityType + N''Id
			    INNER JOIN Skill s ON es.SkillId = s.SkillId
			    LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		    WHERE
			    sg.[Guid] IS NULL
			    AND e.'' + @EntityType + N''Id = @EntityId
			    AND es.IsDeleted = 0''
 		    SET @ParameterDefinition = N''@ModifyGuid UNIQUEIDENTIFIER, @EntityId INT, @SkillGuids dbo.GuidList readonly''
		    EXECUTE sp_executesql @SqlString, @ParameterDefinition, @ModifyGuid = @ModifyGuid, @EntityId = @EntityId, @SkillGuids = @SkillGuids
        END
	END TRY
	BEGIN CATCH
		SELECT 
			 ERROR_NUMBER() AS ErrorNumbere
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
            migrationBuilder.DropTable(
                name: "CourseSkill");

            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Update_EntitySkills]
            ");
        }
    }
}
