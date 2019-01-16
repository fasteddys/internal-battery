using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscriberskillsPKandnavigationproperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriberSkill",
                table: "SubscriberSkill");

            migrationBuilder.DropIndex(
                name: "IX_SubscriberSkill_SkillId",
                table: "SubscriberSkill");

            migrationBuilder.DropColumn(
                name: "SubscriberSkillId",
                table: "SubscriberSkill");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriberSkill",
                table: "SubscriberSkill",
                columns: new[] { "SkillId", "SubscriberId" });

            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2018.12.13 - Bill Koenig - Created
2018.12.18 - Bill Koenig - Removed `saved skills which already exist should be updated` logic (it does not make sense to update 
    data which has not been altered). Added ModifyDate and ModifyGuid to the Subscriber update operation.
2019.01.04 - Bill Koenig - Added code below the `saved skills which were previously deleted should be made active` comment now 
	that the primary key on dbo.SubscriberSkill includes IsDeleted 
</remarks>
<description>
Handles updates to a subscriber (and related entities).
</description>
<example>
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids
VALUES (''3E15B292-6E44-40AA-A1FC-C2106291A286'') -- existing skill
INSERT INTO @SkillGuids
VALUES (''3B0BE2F1-3A52-4671-B9AB-54C42507B4EA'') -- new skill
INSERT INTO @SkillGuids
VALUES (''A191D0F5-53E8-462D-A6CA-4FF3B1E21258'') -- previously deleted skill
EXEC [dbo].[System_Update_Subscriber] @SubscriberGuid = ''47568E38-A8D5-440E-B613-1C0C75787E90'', @FirstName = ''Test1'', 
@LastName = ''Test2'', @Address = ''123 Main St'', @City = ''Baltimore'', @StateGuid = ''0B3CB9E2-02CB-4F46-A1DE-A46FF51C2993'', 
@PhoneNumber = ''4105559999'', @SkillGuids = @SkillGuids
</example>
*/
ALTER PROCEDURE [dbo].[System_Update_Subscriber] (
	@SubscriberGuid UNIQUEIDENTIFIER,
	@FirstName NVARCHAR(MAX) = NULL,
	@LastName NVARCHAR(MAX) = NULL,
	@Address NVARCHAR(MAX) = NULL,
	@City NVARCHAR(MAX) = NULL,
	@StateGuid UNIQUEIDENTIFIER = NULL,
	@PhoneNumber NVARCHAR(MAX) = NULL,
	@FacebookUrl NVARCHAR(MAX) = NULL,
	@TwitterUrl NVARCHAR(MAX) = NULL,
	@LinkedInUrl NVARCHAR(MAX) = NULL,
	@StackOverflowUrl NVARCHAR(MAX) = NULL,
	@GithubUrl NVARCHAR(MAX) = NULL,
	@SkillGuids dbo.GuidList READONLY
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

		-- update basic subscriber properties
		UPDATE Subscriber
		SET FirstName = @FirstName
			, LastName = @LastName
			, [Address] = @Address
			, City = @City
			, StateId = (SELECT TOP 1 s.StateId FROM [State] s WHERE s.StateGuid = @StateGuid)
			, PhoneNumber = @PhoneNumber
			, FacebookUrl = @FacebookUrl 
			, TwitterUrl = @TwitterUrl
			, LinkedInUrl = @LinkedInUrl
			, StackOverflowUrl = @StackOverflowUrl
			, GithubUrl = @GithubUrl
            , ModifyGuid = @ModifyGuid
            , ModifyDate = GETUTCDATE()
		WHERE SubscriberGuid = @SubscriberGuid

		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillGuid
			FROM SubscriberSkill ss 
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			WHERE p.SubscriberGuid = @SubscriberGuid
		)
		INSERT INTO dbo.SubscriberSkill (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, SubscriberId, SkillId, SubscriberSkillGuid)
		SELECT 0, GETUTCDATE(), GETUTCDATE(), @CreateGuid, @ModifyGuid, @SubscriberId, s.SkillId, NEWID()
		FROM @SkillGuids sg
		INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		WHERE es.SkillGuid IS NULL

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
			INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
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
			LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			sg.[Guid] IS NULL
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

END
')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriberSkill",
                table: "SubscriberSkill");

            migrationBuilder.AddColumn<int>(
                name: "SubscriberSkillId",
                table: "SubscriberSkill",
                nullable: false)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriberSkill",
                table: "SubscriberSkill",
                column: "SubscriberSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberSkill_SkillId",
                table: "SubscriberSkill",
                column: "SkillId");

            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2018.12.13 - Bill Koenig - Created
2018.12.18 - Bill Koenig - Removed `saved skills which already exist should be updated` logic (it does not make sense to update 
    data which has not been altered). Added ModifyDate and ModifyGuid to the Subscriber update operation.
</remarks>
<description>
Handles updates to a subscriber (and related entities).
</description>
<example>
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids
VALUES (''3E15B292-6E44-40AA-A1FC-C2106291A286'') -- existing skill
INSERT INTO @SkillGuids
VALUES (''3B0BE2F1-3A52-4671-B9AB-54C42507B4EA'') -- new skill
EXEC [dbo].[System_Update_Subscriber] @SubscriberGuid = ''47568E38-A8D5-440E-B613-1C0C75787E90'', @FirstName = ''Test1'', 
@LastName = ''Test2'', @Address = ''123 Main St'', @City = ''Baltimore'', @StateGuid = ''0B3CB9E2-02CB-4F46-A1DE-A46FF51C2993'', 
@PhoneNumber = ''4105559999'', @SkillGuids = @SkillGuids
</example>
*/
ALTER PROCEDURE [dbo].[System_Update_Subscriber] (
	@SubscriberGuid UNIQUEIDENTIFIER,
	@FirstName NVARCHAR(MAX) = NULL,
	@LastName NVARCHAR(MAX) = NULL,
	@Address NVARCHAR(MAX) = NULL,
	@City NVARCHAR(MAX) = NULL,
	@StateGuid UNIQUEIDENTIFIER = NULL,
	@PhoneNumber NVARCHAR(MAX) = NULL,
	@FacebookUrl NVARCHAR(MAX) = NULL,
	@TwitterUrl NVARCHAR(MAX) = NULL,
	@LinkedInUrl NVARCHAR(MAX) = NULL,
	@StackOverflowUrl NVARCHAR(MAX) = NULL,
	@GithubUrl NVARCHAR(MAX) = NULL,
	@SkillGuids dbo.GuidList READONLY
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

		-- update basic subscriber properties
		UPDATE Subscriber
		SET FirstName = @FirstName
			, LastName = @LastName
			, [Address] = @Address
			, City = @City
			, StateId = (SELECT TOP 1 s.StateId FROM [State] s WHERE s.StateGuid = @StateGuid)
			, PhoneNumber = @PhoneNumber
			, FacebookUrl = @FacebookUrl 
			, TwitterUrl = @TwitterUrl
			, LinkedInUrl = @LinkedInUrl
			, StackOverflowUrl = @StackOverflowUrl
			, GithubUrl = @GithubUrl
            , ModifyGuid = @ModifyGuid
            , ModifyDate = GETUTCDATE()
		WHERE SubscriberGuid = @SubscriberGuid

		-- saved skills which do not exist should be created
		;WITH existingSkills AS (
			SELECT s.SkillId, s.SkillGuid
			FROM SubscriberSkill ss 
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			WHERE p.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0
		)
		INSERT INTO dbo.SubscriberSkill (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, SubscriberId, SkillId, SubscriberSkillGuid)
		SELECT 0, GETUTCDATE(), GETUTCDATE(), @CreateGuid, @ModifyGuid, @SubscriberId, s.SkillId, NEWID()
		FROM @SkillGuids sg
		INNER JOIN Skill s ON sg.[Guid] = s.SkillGuid
		LEFT JOIN existingSkills es ON sg.[Guid] = es.SkillGuid
		WHERE es.SkillGuid IS NULL
	
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
			LEFT JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE
			sg.[Guid] IS NULL
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

END
')
            ");
        }
    }
}
