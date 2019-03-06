using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class correctimportactionnamesinsproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.02.27 - Bill Koenig - Created
2019.03.04 - Bill Koenig - Correct import action names (made them past tense, eg. Ignored, Inserted, Updated)
</remarks>
<description>
Handles the import of a contact into the system. This process evaluates whether the contact already exists and handles the create and update operation
accordingly. Information is returned to the caller indicating what action was taken (Ignored, Inserted, Updated, Error) and a corresponding reason (for errors).
</description>
<example>
DECLARE @ImportActionOutput NVARCHAR(10), @ReasonOutput NVARCHAR(500)
EXEC [dbo].[System_Import_Contact] 
	@PartnerGuid = ''C250AE21-2A81-4659-A05E-59DE90B12AF9'', 
	@Email = ''test@email.com'', 
	@SourceSystemIdentifier = ''99999'', 
	@Metadata = ''{""FirstName"":""Timothy"",""LastName"":""Zimmerman"",""Market"":""Baltimore\/Washington"",""IsEmailSent"":true,""IsNudgeEmailSent"":false}'',
	@ImportAction = @ImportActionOutput OUTPUT,
	@Reason = @ReasonOutput OUTPUT
SELECT @ImportActionOutput [ImportAction], @ReasonOutput [Reason]
</example>
*/
ALTER PROCEDURE [dbo].[System_Import_Contact] (
	@PartnerGuid UNIQUEIDENTIFIER,
	@Email NVARCHAR(450),
	@SourceSystemIdentifier NVARCHAR(MAX),
	@Metadata NVARCHAR(MAX),
	@ImportAction NVARCHAR(10) OUTPUT,
	@Reason NVARCHAR(500) OUTPUT
)
AS
BEGIN	
	SET NOCOUNT ON
	-- this should never get returned; if it does, something went wrong with the logic
	SET @ImportAction = ''Ignored''
	
	BEGIN TRANSACTION;
	BEGIN TRY
		
		-- this is the primary key of the partner associated with the contact we are importing
		DECLARE @PartnerId INT = (SELECT TOP 1 PartnerId FROM dbo.[Partner] WHERE PartnerGuid = @PartnerGuid)

		-- clean up any leading or trailing white space before matching on white space
		SET @Email = LTRIM(RTRIM(@Email))
		
		-- these variables will be used to evaluate whether or not the contact being imported should be an insert or update operation
		DECLARE @ExistingPartnerId INT
		DECLARE @ExistingContactId INT

		-- set the variables defined above 
		;WITH partnerContacts AS (
			SELECT p.PartnerId, pc.ContactId
			FROM dbo.[Partner] p
			INNER JOIN dbo.PartnerContact pc ON p.PartnerId = pc.PartnerId
			WHERE p.PartnerGuid = @PartnerGuid
		)		
		SELECT TOP 1 @ExistingPartnerId = pc.PartnerId, @ExistingContactId = c.ContactId
		FROM dbo.Contact c
		LEFT JOIN partnerContacts pc ON c.ContactId = pc.ContactId 
		WHERE c.Email = @Email

		
		IF @ExistingContactId IS NULL
		BEGIN			
			-- contact does not exist for any partner; insert the contact and partner contact record
			DECLARE @NewContact TABLE (ContactId INT)
			INSERT INTO dbo.Contact (IsDeleted, CreateDate, CreateGuid, Email, ContactGuid)
			OUTPUT inserted.ContactId into @NewContact(ContactId)
			VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @Email, NEWID())

			INSERT INTO dbo.PartnerContact (IsDeleted, CreateDate, CreateGuid, PartnerId, ContactId, SourceSystemIdentifier, MetaDataJSON)
			VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @PartnerId, (SELECT TOP 1 ContactId FROM @NewContact), @SourceSystemIdentifier, @Metadata)

			-- set the import action
			SET @ImportAction = ''Inserted''
		END
		ELSE 
		BEGIN
			IF @ExistingPartnerId IS NULL
			BEGIN
				-- contact exists but not for this partner; insert the partner contact record
				INSERT INTO dbo.PartnerContact (IsDeleted, CreateDate, CreateGuid, PartnerId, ContactId, SourceSystemIdentifier, MetaDataJSON)
				VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @PartnerId, @ExistingContactId, @SourceSystemIdentifier, @Metadata)
			END
			ELSE
			BEGIN
				-- contact exists for this partner; update partner contact record
				UPDATE dbo.PartnerContact
				SET SourceSystemIdentifier = @SourceSystemIdentifier,
					MetaDataJSON = @Metadata,
                    ModifyDate = GETUTCDATE(),
                    ModifyGuid = ''00000000-0000-0000-0000-000000000000''
				WHERE PartnerId = @ExistingPartnerId
				AND ContactId = @ExistingContactId
			END

			-- update the contact record and set the import action
			UPDATE dbo.Contact
			SET ModifyDate = GETUTCDATE(),
				ModifyGuid = ''00000000-0000-0000-0000-000000000000''
			WHERE ContactId = @ExistingContactId			 
			SET @ImportAction = ''Updated''
		END

	END TRY
	BEGIN CATCH
		
		-- set the import action and reason
		SET @ImportAction = ''Error''
		SET @Reason = ERROR_MESSAGE()

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

            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.02.27 - Bill Koenig - Created
</remarks>
<description>
Handles the import of a contact into the system. This process evaluates whether the contact already exists and handles the create and update operation
accordingly. Information is returned to the caller indicating what action was taken (Ignore, Insert, Update, Error) and a corresponding reason (for errors).
</description>
<example>
DECLARE @ImportActionOutput NVARCHAR(10), @ReasonOutput NVARCHAR(500)
EXEC [dbo].[System_Import_Contact] 
	@PartnerGuid = ''C250AE21-2A81-4659-A05E-59DE90B12AF9'', 
	@Email = ''test@email.com'', 
	@SourceSystemIdentifier = ''99999'', 
	@Metadata = ''{""FirstName"":""Timothy"",""LastName"":""Zimmerman"",""Market"":""Baltimore\/Washington"",""IsEmailSent"":true,""IsNudgeEmailSent"":false}'',
	@ImportAction = @ImportActionOutput OUTPUT,
	@Reason = @ReasonOutput OUTPUT
SELECT @ImportActionOutput [ImportAction], @ReasonOutput [Reason]
</example>
*/
CREATE PROCEDURE [dbo].[System_Import_Contact] (
	@PartnerGuid UNIQUEIDENTIFIER,
	@Email NVARCHAR(450),
	@SourceSystemIdentifier NVARCHAR(MAX),
	@Metadata NVARCHAR(MAX),
	@ImportAction NVARCHAR(10) OUTPUT,
	@Reason NVARCHAR(500) OUTPUT
)
AS
BEGIN	
	SET NOCOUNT ON
	-- this should never get returned; if it does, something went wrong with the logic
	SET @ImportAction = ''Ignore''
	
	BEGIN TRANSACTION;
	BEGIN TRY
		
		-- this is the primary key of the partner associated with the contact we are importing
		DECLARE @PartnerId INT = (SELECT TOP 1 PartnerId FROM dbo.[Partner] WHERE PartnerGuid = @PartnerGuid)

		-- clean up any leading or trailing white space before matching on white space
		SET @Email = LTRIM(RTRIM(@Email))
		
		-- these variables will be used to evaluate whether or not the contact being imported should be an insert or update operation
		DECLARE @ExistingPartnerId INT
		DECLARE @ExistingContactId INT

		-- set the variables defined above 
		;WITH partnerContacts AS (
			SELECT p.PartnerId, pc.ContactId
			FROM dbo.[Partner] p
			INNER JOIN dbo.PartnerContact pc ON p.PartnerId = pc.PartnerId
			WHERE p.PartnerGuid = @PartnerGuid
		)		
		SELECT TOP 1 @ExistingPartnerId = pc.PartnerId, @ExistingContactId = c.ContactId
		FROM dbo.Contact c
		LEFT JOIN partnerContacts pc ON c.ContactId = pc.ContactId 
		WHERE c.Email = @Email

		
		IF @ExistingContactId IS NULL
		BEGIN			
			-- contact does not exist for any partner; insert the contact and partner contact record
			DECLARE @NewContact TABLE (ContactId INT)
			INSERT INTO dbo.Contact (IsDeleted, CreateDate, CreateGuid, Email, ContactGuid)
			OUTPUT inserted.ContactId into @NewContact(ContactId)
			VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @Email, NEWID())

			INSERT INTO dbo.PartnerContact (IsDeleted, CreateDate, CreateGuid, PartnerId, ContactId, SourceSystemIdentifier, MetaDataJSON)
			VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @PartnerId, (SELECT TOP 1 ContactId FROM @NewContact), @SourceSystemIdentifier, @Metadata)

			-- set the import action
			SET @ImportAction = ''Insert''
		END
		ELSE 
		BEGIN
			IF @ExistingPartnerId IS NULL
			BEGIN
				-- contact exists but not for this partner; insert the partner contact record
				INSERT INTO dbo.PartnerContact (IsDeleted, CreateDate, CreateGuid, PartnerId, ContactId, SourceSystemIdentifier, MetaDataJSON)
				VALUES (0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @PartnerId, @ExistingContactId, @SourceSystemIdentifier, @Metadata)
			END
			ELSE
			BEGIN
				-- contact exists for this partner; update partner contact record
				UPDATE dbo.PartnerContact
				SET SourceSystemIdentifier = @SourceSystemIdentifier,
					MetaDataJSON = @Metadata,
                    ModifyDate = GETUTCDATE(),
                    ModifyGuid = ''00000000-0000-0000-0000-000000000000''
				WHERE PartnerId = @ExistingPartnerId
				AND ContactId = @ExistingContactId
			END

			-- update the contact record and set the import action
			UPDATE dbo.Contact
			SET ModifyDate = GETUTCDATE(),
				ModifyGuid = ''00000000-0000-0000-0000-000000000000''
			WHERE ContactId = @ExistingContactId			 
			SET @ImportAction = ''Update''
		END

	END TRY
	BEGIN CATCH
		
		-- set the import action and reason
		SET @ImportAction = ''Error''
		SET @Reason = ERROR_MESSAGE()

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
