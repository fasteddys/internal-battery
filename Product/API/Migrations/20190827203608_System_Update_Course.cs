using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class System_Update_Course : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.08.26 - Bill Koenig - Created
</remarks>
<description>
Handles inserts and updates to a course. Note that this only supports a single course variant for now.
</description>
<example>
DECLARE @TagGuids AS [dbo].[GuidList]
INSERT INTO @TagGuids VALUES (''512BE269-217B-41FB-A636-A96B9B593A24'') 
INSERT INTO @TagGuids VALUES (''19614136-5FA7-45E1-B081-5EE0308F7786'') 

DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids VALUES (''3F5C13F7-AACB-4499-A646-1BF8A653FBDE'')
INSERT INTO @SkillGuids VALUES (''3F5C13F7-AACB-4499-A646-1BF8A653FBDE'')
INSERT INTO @SkillGuids VALUES (''5D611683-E035-4B40-B7A6-4B94264E731F'') 

DECLARE @CourseId INT
DECLARE @ErrorLine NVARCHAR(MAX)
DECLARE @ErrorMessage NVARCHAR(MAX)
DECLARE @ErrorProcedure NVARCHAR(MAX)

EXEC [dbo].[System_Update_Course] 
	@CourseId = @CourseId OUTPUT, 
	@CourseVariantTypeGuid = ''834C730B-8E00-4916-9CD3-7760446FE298'', 
	@VendorGuid = ''96229316-D51D-4A00-8FEB-E3EF0417C4EF'',
	@Price = 10.00,
	@Code = ''Test course code #6'',
	@Name = ''Test course name #6'',
	@Description = ''Test course description #6'',
	@IsExternal = 1,
	@TagGuids = @TagGuids,
	@SkillGuids = @SkillGuids,
	@ErrorLine = @ErrorLine OUTPUT,
	@ErrorMessage = @ErrorMessage OUTPUT,
	@ErrorProcedure = @ErrorProcedure OUTPUT

SELECT @CourseId [CourseId], @ErrorLine [ErrorLine], @ErrorMessage [ErrorMessage], @ErrorProcedure [ErrorProcedure]
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_Course] (
	@CourseId INT OUTPUT,
	@CourseVariantTypeGuid UNIQUEIDENTIFIER,
	@VendorGuid UNIQUEIDENTIFIER,
	@Price DECIMAL(18,2),
	@Name NVARCHAR(MAX),
	@Code NVARCHAR(MAX) = NULL,
	@Description NVARCHAR(MAX) = NULL,
	@IsExternal BIT,
	@TagGuids dbo.GuidList READONLY,
	@SkillGuids dbo.GuidList READONLY,
	@ErrorLine NVARCHAR(MAX) OUTPUT,
	@ErrorMessage NVARCHAR(MAX) OUTPUT,
	@ErrorProcedure NVARCHAR(MAX) OUTPUT
)
AS
BEGIN

	BEGIN TRANSACTION;

	BEGIN TRY
		
		-- replace these with meaningful values (once we have a mechanism in place to generate them)
		DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
		DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''

		-- lookup the course variant type id using the provided guid
		DECLARE @CourseVariantTypeId INT = (SELECT TOP 1 cvt.CourseVariantTypeId FROM CourseVariantType cvt WHERE cvt.CourseVariantGuid = @CourseVariantTypeGuid)

		-- lookup the vendor id using the provided guid
		DECLARE @VendorId INT = (SELECT TOP 1 v.VendorId FROM Vendor v WHERE v.VendorGuid = @VendorGuid)

		DECLARE @CourseOutput table (CourseId INT)
		IF(@CourseId IS NULL)
		BEGIN
			INSERT INTO dbo.Course(CourseGuid, [Name], VendorId, Code, IsDeleted, [Description], CreateDate, CreateGuid, IsExternal)
			OUTPUT INSERTED.CourseId INTO @CourseOutput(CourseId)
			VALUES (NEWID(), @Name, @VendorId, @Code, 0, @Description, GETUTCDATE(), @CreateGuid, @IsExternal)
			SET @CourseId = (SELECT TOP 1 CourseId FROM @CourseOutput)

			INSERT INTO dbo.CourseVariant(IsDeleted, CreateDate, CreateGuid, Price, CourseVariantGuid, CourseId, CourseVariantTypeId)
			VALUES (0, GETUTCDATE(), @CreateGuid, @Price, NEWID(), @CourseId, @CourseVariantTypeId)
		END
		ELSE
		BEGIN
			UPDATE dbo.Course
			SET [Name] = @Name, [Description] = @Description, Code = @Code, IsExternal = @IsExternal, VendorId = @VendorId, ModifyGuid = @ModifyGuid, ModifyDate = GETUTCDATE()
			WHERE CourseId = @CourseId

			UPDATE dbo.CourseVariant
			SET ModifyDate = GETUTCDATE(), ModifyGuid = @ModifyGuid, Price = @Price, CourseVariantTypeId = @CourseVariantTypeId
			WHERE CourseId = @CourseId
		END
				
		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillGuid
			FROM CourseSkill cs 
			INNER JOIN Skill s ON cs.SkillId = s.SkillId
			WHERE cs.CourseId = @CourseId
		)
		INSERT INTO dbo.CourseSkill(IsDeleted, CreateDate, CreateGuid, CourseId, SkillId, CourseSkillGuid)
		SELECT 0, GETUTCDATE(), @CreateGuid, @CourseId, s.SkillId, NEWID()
		FROM @SkillGuids sg
		INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		WHERE es.SkillGuid IS NULL

		-- saved skills which were previously deleted should be made active
		UPDATE 
			cs
		SET
			cs.IsDeleted = 0
			, cs.ModifyDate = GETUTCDATE()
			, cs.ModifyGuid = @ModifyGuid
		FROM  
			CourseSkill cs
			INNER JOIN Skill s ON cs.SkillId = s.SkillId
			INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			cs.CourseId = @CourseId
			AND cs.IsDeleted = 1

		-- existing skills not specified in saved skills should be logically deleted
		UPDATE 
			cs
		SET
			cs.IsDeleted = 1
			, cs.ModifyDate = GETUTCDATE()
			, cs.ModifyGuid = @ModifyGuid
		FROM  
			CourseSkill cs			
			INNER JOIN Skill s ON cs.SkillId = s.SkillId
			LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			sg.[Guid] IS NULL
			AND cs.CourseId = @CourseId
			AND cs.IsDeleted = 0

		-- saved tags which do not exist should be created
		;WITH existingTags AS (
			SELECT t.TagId, t.TagGuid
			FROM TagCourse tc 
			INNER JOIN Tag t ON tc.TagId = t.TagId
			WHERE tc.CourseId = @CourseId
		)
		INSERT INTO dbo.TagCourse(IsDeleted, CreateDate, CreateGuid, CourseId, TagId, TagCourseGuid)
		SELECT 0, GETUTCDATE(), @CreateGuid, @CourseId, t.TagId, NEWID()
		FROM @TagGuids tg
		INNER JOIN Tag t ON tg.[Guid] = t.TagGuid
		LEFT JOIN existingTags et ON tg.[Guid] = et.TagGuid
		WHERE et.TagGuid IS NULL

		-- saved tags which were previously deleted should be made active
		UPDATE 
			tc
		SET
			tc.IsDeleted = 0
			, tc.ModifyDate = GETUTCDATE()
			, tc.ModifyGuid = @ModifyGuid
		FROM  
			TagCourse tc
			INNER JOIN Tag t ON tc.TagId = t.TagId
			INNER JOIN @TagGuids tg ON t.TagGuid = tg.[Guid]
		WHERE
			tc.CourseId = @CourseId
			AND tc.IsDeleted = 1

		-- existing tags not specified in saved tags should be logically deleted
		UPDATE 
			tc
		SET
			tc.IsDeleted = 1
			, tc.ModifyDate = GETUTCDATE()
			, tc.ModifyGuid = @ModifyGuid
		FROM  
			TagCourse tc			
			INNER JOIN Tag t ON tc.TagId = t.TagId
			LEFT JOIN @TagGuids tg ON t.TagGuid = tg.[Guid]
		WHERE
			tg.[Guid] IS NULL
			AND tc.CourseId = @CourseId
			AND tc.IsDeleted = 0
 
	END TRY
	BEGIN CATCH
		SET @CourseId = NULL
		SET @ErrorProcedure = ERROR_PROCEDURE()
		SET @ErrorLine = ERROR_LINE()
		SET @ErrorMessage = ERROR_MESSAGE()

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
	END CATCH;

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION;

END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Update_Course]");
        }
    }
}
