using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class newstoredprocforsubscriberskillupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/*
<remarks>
2018.12.13 - Bill Koenig - Created
</remarks>
<description>
A simple user-defined table type that supports a list of Guids.
</description>
*/
CREATE TYPE [dbo].[GuidList] AS TABLE ([Guid] UNIQUEIDENTIFIER)
GO

/*
<remarks>
2018.12.13 - Bill Koenig - Created
</remarks>
<description>
Handles updates to a subscriber (and related entities).
</description>
<example>
DECLARE @SkillGuids AS [dbo].[GuidList]
INSERT INTO @SkillGuids
VALUES ('3E15B292-6E44-40AA-A1FC-C2106291A286') -- existing skill
INSERT INTO @SkillGuids
VALUES ('3B0BE2F1-3A52-4671-B9AB-54C42507B4EA') -- new skill
EXEC [dbo].[System_Update_Subscriber] @SubscriberGuid = '47568E38-A8D5-440E-B613-1C0C75787E90', @FirstName = 'Test1', @LastName = 'Test2', @Address = '123 Main St', @City = 'Baltimore', @StateGuid = '0B3CB9E2-02CB-4F46-A1DE-A46FF51C2993', @PhoneNumber = '4105559999', @SkillGuids = @SkillGuids
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_Subscriber] (
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
		DECLARE @CreateGuid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'
		DECLARE @ModifyGuid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'

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
		WHERE SubscriberGuid = @SubscriberGuid

		-- saved skills which already exist should be updated
		UPDATE 
			ss
		SET
			ss.ModifyDate = GETUTCDATE()
			, ss.ModifyGuid = @ModifyGuid
		FROM  
			SubscriberSkill ss
			INNER JOIN Subscriber p ON ss.SubscriberId = p.SubscriberId
			INNER JOIN Skill s ON ss.SkillId = s.SkillId
			INNER JOIN @SkillGuids sg ON s.SkillGuid = sg.[Guid]
		WHERE p.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0

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
GO"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Update_Subscriber]
GO

DROP TYPE [dbo].[GuidList]
GO"
            );
        }
    }
}
