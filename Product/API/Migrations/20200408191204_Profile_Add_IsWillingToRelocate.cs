using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Profile_Add_IsWillingToRelocate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWillingToRelocate",
                schema: "G2",
                table: "Profiles",
                nullable: true);

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many)
2020.03.31 - Joey Herrington - Added ''SkillsNote'' to the input list
2020.04.08 - Joey Herrington - Added ''IsWillingToRelocate'' to the input list
</remarks>
<description>
Creates a profile
</description>
<example>
DECLARE @ProfileGuid UNIQUEIDENTIFIER
DECLARE @ValidationErrors NVARCHAR(MAX)
DECLARE @EmploymentTypeGuids AS [dbo].[GuidList]
INSERT INTO @EmploymentTypeGuids VALUES (''EB3F2DB6-AAA9-4660-A92D-A11E4D83F23A'')
INSERT INTO @EmploymentTypeGuids VALUES (''FA5744CD-D1C7-4F42-9A58-CF1B0FB28997'')
EXEC [G2].[System_Create_Profile] @CompanyGuid = ''C8614494-7D20-459C-BBD1-DA87F9D4B5F6''
, @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @CityGuid = ''66A83A17-0776-4B19-85FD-B17B06517812''
, @StateGuid = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC''
, @PostalGuid = ''B35BB598-B4B6-4A13-8187-D58491BE811D''
, @ExperienceLevelGuid = ''F3E95E74-1CC4-4547-8F29-11DE077514C1''
, @ContactTypeGuid = ''1C69165E-8005-4E62-8E70-E4EC4EAFCEFB''
, @FirstName = ''Jason''
, @LastName = ''Carr''
, @Email = ''jasonrc42@gmail.com''
, @PhoneNumber = ''4109310684''
, @StreetAddress = ''917 Tartan Hill Dr''
, @Title = ''Electrical Engineer''
, @IsWillingToTravel = 0
, @IsWillingToRelocate = 1
, @IsActiveJobSeeker = 0
, @IsCurrentlyEmployed = 1
, @IsWillingToWorkProBono = 0
, @CurrentRate = 45.75
, @DesiredRate = 60.00
, @Goals = ''Testing goals''
, @Preferences = ''Testing preferences''
, @SkillsNote = ''I have skills''
, @EmploymentTypeGuids = @EmploymentTypeGuids
, @ProfileGuid = @ProfileGuid OUTPUT
, @ValidationErrors = @ValidationErrors OUTPUT
SELECT @ValidationErrors ValidationErrors
</example>
*/
ALTER PROCEDURE [G2].[System_Create_Profile] (
    @CompanyGuid UNIQUEIDENTIFIER, 
	@SubscriberGuid UNIQUEIDENTIFIER, 
	@CityGuid UNIQUEIDENTIFIER,
	@StateGuid UNIQUEIDENTIFIER,
	@PostalGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@ContactTypeGuid UNIQUEIDENTIFIER, 
    @FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@Email NVARCHAR(254),
    @PhoneNumber NVARCHAR(20),
    @StreetAddress NVARCHAR(100), 
    @Title NVARCHAR(100),
    @IsWillingToTravel BIT,
    @IsWillingToRelocate BIT,
    @IsActiveJobSeeker BIT,
    @IsCurrentlyEmployed BIT,
    @IsWillingToWorkProBono BIT,
    @CurrentRate DECIMAL(18,2),
    @DesiredRate DECIMAL(18,2),
    @Goals NVARCHAR(500),
	@Preferences NVARCHAR(500),	
	@SkillsNote NVARCHAR(500),
	@EmploymentTypeGuids dbo.GuidList READONLY,
	@ProfileGuid UNIQUEIDENTIFIER OUTPUT, 
    @ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
    -- need to set this to be an empty string otherwise concatenation won''t work
    SET @ValidationErrors = ''''

	IF EXISTS(
		SELECT *
		FROM G2.Profiles p
		INNER JOIN dbo.Subscriber s ON p.SubscriberId = s.SubscriberId
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		WHERE p.IsDeleted = 0
		AND s.IsDeleted = 0
		AND c.IsDeleted = 0
		AND c.CompanyGuid = @CompanyGuid
		AND s.SubscriberGuid = @SubscriberGuid)
		SET @ValidationErrors = @ValidationErrors + ''A profile already exists for the specified company and subscriber;''

    -- declare parameters for foreign keys
    DECLARE @CompanyId INT, @SubscriberId INT, @CityId INT, @StateId INT, @PostalId INT, @ExperienceLevelId INT, @ContactTypeId INT = NULL

    SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
    IF(@CompanyId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''
	
    SET @SubscriberId = (SELECT TOP 1 SubscriberId FROM dbo.Subscriber WHERE IsDeleted = 0 AND SubscriberGuid = @SubscriberGuid)
    IF(@SubscriberId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid subscriber specified (required);''

	IF(@CityGuid IS NOT NULL AND @CityGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @CityId = (SELECT TOP 1 CityId FROM dbo.City WHERE IsDeleted = 0 AND CityGuid = @CityGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid city specified;''
	END
	
	IF(@StateGuid IS NOT NULL AND @StateGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @StateId = (SELECT TOP 1 StateId FROM dbo.[State] WHERE IsDeleted = 0 AND StateGuid = @StateGuid)
		IF(@StateId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid state specified;''
	END

	IF(@PostalGuid IS NOT NULL AND @PostalGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @PostalId = (SELECT TOP 1 PostalId FROM dbo.Postal WHERE IsDeleted = 0 AND PostalGuid = @PostalGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid postal code specified;''
	END

	IF(@ExperienceLevelGuid IS NOT NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
		IF(@ExperienceLevelId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
	END
	
	IF EXISTS(
		SELECT g.[Guid]
		FROM @EmploymentTypeGuids g 
		LEFT JOIN dbo.EmploymentType et ON et.EmploymentTypeGuid = g.[Guid]
		WHERE et.EmploymentTypeGuid IS NULL)
	BEGIN
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type(s) specified;''
	END

	IF(@ContactTypeGuid IS NOT NULL AND @ContactTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ContactTypeId = (SELECT TOP 1 ContactTypeId FROM G2.ContactTypes WHERE IsDeleted = 0 AND ContactTypeGuid = @ContactTypeGuid)
		IF(@ContactTypeId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''
	END

    -- perform insert if there were no validation errors
    IF(LEN(@ValidationErrors) = 0)
    BEGIN
		
		DECLARE @Output TABLE (ProfileId INT)

		SET @ProfileGuid = NEWID()
		INSERT INTO G2.Profiles(IsDeleted
			, CreateDate
			, CreateGuid
			, ProfileGuid
			, CompanyId
			, SubscriberId
			, FirstName
			, LastName
			, Email
			, PhoneNumber
			, ContactTypeId
			, StreetAddress
			, CityId
			, StateId
			, PostalId
			, ExperienceLevelId
			, Title
			, IsWillingToTravel
			, IsWillingToRelocate
			, IsActiveJobSeeker
			, IsCurrentlyEmployed
			, IsWillingToWorkProBono
			, CurrentRate
			, DesiredRate
			, Goals
			, Preferences
			, SkillsNote
			, AzureIndexStatusId)
		OUTPUT INSERTED.ProfileId INTO @Output(ProfileId)
		VALUES (0
			, GETUTCDATE()
			, ''00000000-0000-0000-0000-000000000000''
			, @ProfileGuid
			, @CompanyId
			, @SubscriberId
			, @FirstName
			, @LastName
			, @Email
			, @PhoneNumber
			, @ContactTypeId
			, @StreetAddress
			, @CityId
			, @StateId
			, @PostalId
			, @ExperienceLevelId
			, @Title
			, @IsWillingToTravel
			, @IsWillingToRelocate
			, @IsActiveJobSeeker
			, @IsCurrentlyEmployed
			, @IsWillingToWorkProBono
			, @CurrentRate
			, @DesiredRate
			, @Goals
			, @Preferences
			, @SkillsNote
			, (SELECT TOP 1 AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''None''))

		DECLARE @ProfileId INT = (SELECT TOP 1 ProfileId FROM @Output)
		-- create employment types 
		INSERT INTO G2.ProfileEmploymentTypes(IsDeleted, CreateDate, CreateGuid, ProfileEmploymentTypeGuid, ProfileId, EmploymentTypeId)
		SELECT 0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', NEWID(), @ProfileId, et.EmploymentTypeId
		FROM @EmploymentTypeGuids etg
		INNER JOIN dbo.EmploymentType et ON etg.[Guid] = et.EmploymentTypeGuid

    END	
    ELSE
    BEGIN
    	-- remove the last occurrence of the semicolon
    	SET @ValidationErrors = CASE WHEN CHARINDEX('';'', @ValidationErrors) = 0 THEN @ValidationErrors ELSE LEFT(@ValidationErrors, LEN(@ValidationErrors) - CHARINDEX('';'', REVERSE(@ValidationErrors) + '';'')) END
    END
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.03 - Bill Koenig - Updates AzureSearchIndexInfo to be NULL (since we are setting the status to pending, it wouldn''t make sense to have the previous operation''s info here)
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many)
2020.03.31 - Joey Herrington - Added ''SkillsNote'' to the input list
2020.04.08 - Joey Herrington - Added ''IsWillingToRelocate'' to the input list
</remarks>
<description>
Updates a profile
</description>
<example>
DECLARE @ValidationErrors NVARCHAR(MAX)
DECLARE @EmploymentTypeGuids AS [dbo].[GuidList]
INSERT INTO @EmploymentTypeGuids VALUES (''FB5F293C-24F3-4473-BF47-EA1F1455272C'')
INSERT INTO @EmploymentTypeGuids VALUES (''C912F22A-1376-482E-8A33-9C2868A4366D'')
EXEC [G2].[System_Update_Profile] @RecruiterSubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @ProfileGuid = ''79F222F2-7994-4A53-97F2-72A92C2A2A2D''
, @CompanyGuid = ''326013DA-E39B-4B8E-A8D9-53FEBC9243D4''
, @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @CityGuid = ''66A83A17-0776-4B19-85FD-B17B06517812''
, @StateGuid = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC''
, @PostalGuid = ''B35BB598-B4B6-4A13-8187-D58491BE811D''
, @ExperienceLevelGuid = ''F3E95E74-1CC4-4547-8F29-11DE077514C1''
, @ContactTypeGuid = ''1C69165E-8005-4E62-8E70-E4EC4EAFCEFB''
, @FirstName = ''Jason''
, @LastName = ''Carr''
, @Email = ''jasonrc42@gmail.com''
, @PhoneNumber = ''4109310684''
, @StreetAddress = ''917 Tartan Hill Dr''
, @Title = ''Electrical Engineer''
, @IsWillingToTravel = 0
, @IsWillingToRelocate = 1
, @IsActiveJobSeeker = 0
, @IsCurrentlyEmployed = 1
, @IsWillingToWorkProBono = 0
, @CurrentRate = 45.75
, @DesiredRate = 60.00
, @Goals = ''Testing goals''
, @Preferences = ''Testing preferences''
, @SkillsNote = ''I have skills''
, @EmploymentTypeGuids = @EmploymentTypeGuids
, @ValidationErrors = @ValidationErrors OUTPUT
SELECT @ValidationErrors ValidationErrors
</example>
*/
ALTER PROCEDURE [G2].[System_Update_Profile] (
    @RecruiterSubscriberGuid UNIQUEIDENTIFIER,
	@ProfileGuid UNIQUEIDENTIFIER, 
    @CompanyGuid UNIQUEIDENTIFIER, 
	@SubscriberGuid UNIQUEIDENTIFIER, 
	@CityGuid UNIQUEIDENTIFIER,
	@StateGuid UNIQUEIDENTIFIER,
	@PostalGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@ContactTypeGuid UNIQUEIDENTIFIER, 
    @FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@Email NVARCHAR(254),
    @PhoneNumber NVARCHAR(20),
    @StreetAddress NVARCHAR(100), 
    @Title NVARCHAR(100),
    @IsWillingToTravel BIT,
    @IsWillingToRelocate BIT,
    @IsActiveJobSeeker BIT,
    @IsCurrentlyEmployed BIT,
    @IsWillingToWorkProBono BIT,
    @CurrentRate DECIMAL(18,2),
    @DesiredRate DECIMAL(18,2),
    @Goals NVARCHAR(500),
	@Preferences NVARCHAR(500),
    @SkillsNote NVARCHAR(500),
	@EmploymentTypeGuids dbo.GuidList READONLY,
    @ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
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

    -- declare parameters for foreign keys
    DECLARE @CompanyId INT, @SubscriberId INT, @CityId INT, @StateId INT, @PostalId INT, @ExperienceLevelId INT, @ContactTypeId INT = NULL

    SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
    IF(@CompanyId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''
	
    SET @SubscriberId = (SELECT TOP 1 SubscriberId FROM dbo.Subscriber WHERE IsDeleted = 0 AND SubscriberGuid = @SubscriberGuid)
    IF(@SubscriberId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid subscriber specified (required);''

	IF(@CityGuid IS NOT NULL AND @CityGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @CityId = (SELECT TOP 1 CityId FROM dbo.City WHERE IsDeleted = 0 AND CityGuid = @CityGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid city specified;''
	END
	
	IF(@StateGuid IS NOT NULL AND @StateGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @StateId = (SELECT TOP 1 StateId FROM dbo.[State] WHERE IsDeleted = 0 AND StateGuid = @StateGuid)
		IF(@StateId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid state specified;''
	END

	IF(@PostalGuid IS NOT NULL AND @PostalGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @PostalId = (SELECT TOP 1 PostalId FROM dbo.Postal WHERE IsDeleted = 0 AND PostalGuid = @PostalGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid postal code specified;''
	END

	IF(@ExperienceLevelGuid IS NOT NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
		IF(@ExperienceLevelId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
	END

	IF EXISTS(
		SELECT g.[Guid]
		FROM @EmploymentTypeGuids g 
		LEFT JOIN dbo.EmploymentType et ON et.EmploymentTypeGuid = g.[Guid]
		WHERE et.EmploymentTypeGuid IS NULL)
	BEGIN
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type(s) specified;''
	END

	IF(@ContactTypeGuid IS NOT NULL AND @ContactTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ContactTypeId = (SELECT TOP 1 ContactTypeId FROM G2.ContactTypes WHERE IsDeleted = 0 AND ContactTypeGuid = @ContactTypeGuid)
		IF(@ContactTypeId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''
	END

    -- perform insert if there were no validation errors
    IF(LEN(@ValidationErrors) = 0)
    BEGIN
    	UPDATE G2.Profiles
    	SET ModifyDate = GETUTCDATE()
    		, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
    		, CompanyId = @CompanyId
			, SubscriberId = @SubscriberId
			, CityId = @CityId
			, StateId = @StateId
			, PostalId = @PostalId
			, ExperienceLevelId = @ExperienceLevelId
			, ContactTypeId = @ContactTypeId
			, FirstName = @FirstName
			, LastName = @LastName
			, Email = @Email
			, PhoneNumber = @PhoneNumber
			, StreetAddress = @StreetAddress
			, Title = @Title
			, IsWillingToTravel = @IsWillingToTravel
			, IsWillingToRelocate = @IsWillingToRelocate
			, IsActiveJobSeeker = @IsActiveJobSeeker
			, IsCurrentlyEmployed = @IsCurrentlyEmployed
			, IsWillingToWorkProBono = @IsWillingToWorkProBono
			, CurrentRate = @CurrentRate
			, DesiredRate = @DesiredRate
			, Goals = @Goals
			, Preferences = @Preferences
			, SkillsNote = @SkillsNote
			, AzureIndexStatusId = (SELECT TOP 1 AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''Pending'')
			, AzureSearchIndexInfo = NULL
    	WHERE ProfileGuid = @ProfileGuid

		DECLARE @ProfileId INT = (SELECT TOP 1 ProfileId FROM G2.Profiles WHERE ProfileGuid = @ProfileGuid)
		-- saved employment types which do not exist should be created
		;WITH existingEmploymentTypes AS (
			SELECT et.EmploymentTypeId, et.EmploymentTypeGuid
			FROM G2.ProfileEmploymentTypes pet 
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			WHERE pet.ProfileId = @ProfileId
		)
		INSERT INTO G2.ProfileEmploymentTypes (IsDeleted, CreateDate, CreateGuid, ProfileId, EmploymentTypeId, ProfileEmploymentTypeGuid)
		SELECT 0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @ProfileId, et.EmploymentTypeId, NEWID()
		FROM @EmploymentTypeGuids etg
		INNER JOIN dbo.EmploymentType et ON etg.[Guid] = et.EmploymentTypeGuid
		LEFT JOIN existingEmploymentTypes eet ON etg.[Guid] = eet.EmploymentTypeGuid
		WHERE eet.EmploymentTypeGuid IS NULL

		-- saved employment types which were previously deleted should be made active
		UPDATE 
			pet
		SET
			pet.IsDeleted = 0
			, pet.ModifyDate = GETUTCDATE()
			, pet.ModifyGuid = ''00000000-0000-0000-0000-000000000000''
		FROM  
			G2.ProfileEmploymentTypes pet
			INNER JOIN G2.Profiles p ON pet.ProfileId = p.ProfileId
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			INNER JOIN @EmploymentTypeGuids etg ON et.EmploymentTypeGuid = etg.[Guid]
		WHERE
			p.ProfileId = @ProfileId
			AND pet.IsDeleted = 1

		-- existing employment types not specified in saved employment types should be logically deleted
		UPDATE 
			pet
		SET
			pet.IsDeleted = 1
			, pet.ModifyDate = GETUTCDATE()
			, pet.ModifyGuid = ''00000000-0000-0000-0000-000000000000''
		FROM  
			G2.ProfileEmploymentTypes pet
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			LEFT JOIN @EmploymentTypeGuids etg ON et.EmploymentTypeGuid = etg.[Guid]
		WHERE
			etg.[Guid] IS NULL
			AND pet.ProfileId = @ProfileId
			AND pet.IsDeleted = 0

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
            migrationBuilder.DropColumn(
                name: "IsWillingToRelocate",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many)
2020.03.31 - Joey Herrington - Added ''SkillsNote'' to the input list
</remarks>
<description>
Creates a profile
</description>
<example>
DECLARE @ProfileGuid UNIQUEIDENTIFIER
DECLARE @ValidationErrors NVARCHAR(MAX)
DECLARE @EmploymentTypeGuids AS [dbo].[GuidList]
INSERT INTO @EmploymentTypeGuids VALUES (''EB3F2DB6-AAA9-4660-A92D-A11E4D83F23A'')
INSERT INTO @EmploymentTypeGuids VALUES (''FA5744CD-D1C7-4F42-9A58-CF1B0FB28997'')
EXEC [G2].[System_Create_Profile] @CompanyGuid = ''C8614494-7D20-459C-BBD1-DA87F9D4B5F6''
, @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @CityGuid = ''66A83A17-0776-4B19-85FD-B17B06517812''
, @StateGuid = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC''
, @PostalGuid = ''B35BB598-B4B6-4A13-8187-D58491BE811D''
, @ExperienceLevelGuid = ''F3E95E74-1CC4-4547-8F29-11DE077514C1''
, @ContactTypeGuid = ''1C69165E-8005-4E62-8E70-E4EC4EAFCEFB''
, @FirstName = ''Jason''
, @LastName = ''Carr''
, @Email = ''jasonrc42@gmail.com''
, @PhoneNumber = ''4109310684''
, @StreetAddress = ''917 Tartan Hill Dr''
, @Title = ''Electrical Engineer''
, @IsWillingToTravel = 0
, @IsActiveJobSeeker = 0
, @IsCurrentlyEmployed = 1
, @IsWillingToWorkProBono = 0
, @CurrentRate = 45.75
, @DesiredRate = 60.00
, @Goals = ''Testing goals''
, @Preferences = ''Testing preferences''
, @SkillsNote = ''I have skills''
, @EmploymentTypeGuids = @EmploymentTypeGuids
, @ProfileGuid = @ProfileGuid OUTPUT
, @ValidationErrors = @ValidationErrors OUTPUT
SELECT @ValidationErrors ValidationErrors
</example>
*/
ALTER PROCEDURE [G2].[System_Create_Profile] (
    @CompanyGuid UNIQUEIDENTIFIER, 
	@SubscriberGuid UNIQUEIDENTIFIER, 
	@CityGuid UNIQUEIDENTIFIER,
	@StateGuid UNIQUEIDENTIFIER,
	@PostalGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@ContactTypeGuid UNIQUEIDENTIFIER, 
    @FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@Email NVARCHAR(254),
    @PhoneNumber NVARCHAR(20),
    @StreetAddress NVARCHAR(100), 
    @Title NVARCHAR(100),
    @IsWillingToTravel BIT,
    @IsActiveJobSeeker BIT,
    @IsCurrentlyEmployed BIT,
    @IsWillingToWorkProBono BIT,
    @CurrentRate DECIMAL(18,2),
    @DesiredRate DECIMAL(18,2),
    @Goals NVARCHAR(500),
	@Preferences NVARCHAR(500),	
	@SkillsNote NVARCHAR(500),
	@EmploymentTypeGuids dbo.GuidList READONLY,
	@ProfileGuid UNIQUEIDENTIFIER OUTPUT, 
    @ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
    -- need to set this to be an empty string otherwise concatenation won''t work
    SET @ValidationErrors = ''''

	IF EXISTS(
		SELECT *
		FROM G2.Profiles p
		INNER JOIN dbo.Subscriber s ON p.SubscriberId = s.SubscriberId
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		WHERE p.IsDeleted = 0
		AND s.IsDeleted = 0
		AND c.IsDeleted = 0
		AND c.CompanyGuid = @CompanyGuid
		AND s.SubscriberGuid = @SubscriberGuid)
		SET @ValidationErrors = @ValidationErrors + ''A profile already exists for the specified company and subscriber;''

    -- declare parameters for foreign keys
    DECLARE @CompanyId INT, @SubscriberId INT, @CityId INT, @StateId INT, @PostalId INT, @ExperienceLevelId INT, @ContactTypeId INT = NULL

    SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
    IF(@CompanyId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''
	
    SET @SubscriberId = (SELECT TOP 1 SubscriberId FROM dbo.Subscriber WHERE IsDeleted = 0 AND SubscriberGuid = @SubscriberGuid)
    IF(@SubscriberId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid subscriber specified (required);''

	IF(@CityGuid IS NOT NULL AND @CityGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @CityId = (SELECT TOP 1 CityId FROM dbo.City WHERE IsDeleted = 0 AND CityGuid = @CityGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid city specified;''
	END
	
	IF(@StateGuid IS NOT NULL AND @StateGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @StateId = (SELECT TOP 1 StateId FROM dbo.[State] WHERE IsDeleted = 0 AND StateGuid = @StateGuid)
		IF(@StateId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid state specified;''
	END

	IF(@PostalGuid IS NOT NULL AND @PostalGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @PostalId = (SELECT TOP 1 PostalId FROM dbo.Postal WHERE IsDeleted = 0 AND PostalGuid = @PostalGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid postal code specified;''
	END

	IF(@ExperienceLevelGuid IS NOT NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
		IF(@ExperienceLevelId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
	END
	
	IF EXISTS(
		SELECT g.[Guid]
		FROM @EmploymentTypeGuids g 
		LEFT JOIN dbo.EmploymentType et ON et.EmploymentTypeGuid = g.[Guid]
		WHERE et.EmploymentTypeGuid IS NULL)
	BEGIN
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type(s) specified;''
	END

	IF(@ContactTypeGuid IS NOT NULL AND @ContactTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ContactTypeId = (SELECT TOP 1 ContactTypeId FROM G2.ContactTypes WHERE IsDeleted = 0 AND ContactTypeGuid = @ContactTypeGuid)
		IF(@ContactTypeId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''
	END

    -- perform insert if there were no validation errors
    IF(LEN(@ValidationErrors) = 0)
    BEGIN
		
		DECLARE @Output TABLE (ProfileId INT)

		SET @ProfileGuid = NEWID()
		INSERT INTO G2.Profiles(IsDeleted
			, CreateDate
			, CreateGuid
			, ProfileGuid
			, CompanyId
			, SubscriberId
			, FirstName
			, LastName
			, Email
			, PhoneNumber
			, ContactTypeId
			, StreetAddress
			, CityId
			, StateId
			, PostalId
			, ExperienceLevelId
			, Title
			, IsWillingToTravel
			, IsActiveJobSeeker
			, IsCurrentlyEmployed
			, IsWillingToWorkProBono
			, CurrentRate
			, DesiredRate
			, Goals
			, Preferences
			, SkillsNote
			, AzureIndexStatusId)
		OUTPUT INSERTED.ProfileId INTO @Output(ProfileId)
		VALUES (0
			, GETUTCDATE()
			, ''00000000-0000-0000-0000-000000000000''
			, @ProfileGuid
			, @CompanyId
			, @SubscriberId
			, @FirstName
			, @LastName
			, @Email
			, @PhoneNumber
			, @ContactTypeId
			, @StreetAddress
			, @CityId
			, @StateId
			, @PostalId
			, @ExperienceLevelId
			, @Title
			, @IsWillingToTravel
			, @IsActiveJobSeeker
			, @IsCurrentlyEmployed
			, @IsWillingToWorkProBono
			, @CurrentRate
			, @DesiredRate
			, @Goals
			, @Preferences
			, @SkillsNote
			, (SELECT TOP 1 AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''None''))

		DECLARE @ProfileId INT = (SELECT TOP 1 ProfileId FROM @Output)
		-- create employment types 
		INSERT INTO G2.ProfileEmploymentTypes(IsDeleted, CreateDate, CreateGuid, ProfileEmploymentTypeGuid, ProfileId, EmploymentTypeId)
		SELECT 0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', NEWID(), @ProfileId, et.EmploymentTypeId
		FROM @EmploymentTypeGuids etg
		INNER JOIN dbo.EmploymentType et ON etg.[Guid] = et.EmploymentTypeGuid

    END	
    ELSE
    BEGIN
    	-- remove the last occurrence of the semicolon
    	SET @ValidationErrors = CASE WHEN CHARINDEX('';'', @ValidationErrors) = 0 THEN @ValidationErrors ELSE LEFT(@ValidationErrors, LEN(@ValidationErrors) - CHARINDEX('';'', REVERSE(@ValidationErrors) + '';'')) END
    END
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.03 - Bill Koenig - Updates AzureSearchIndexInfo to be NULL (since we are setting the status to pending, it wouldn''t make sense to have the previous operation''s info here)
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many)
2020.03.31 - Joey Herrington - Added ''SkillsNote'' to the input list
</remarks>
<description>
Updates a profile
</description>
<example>
DECLARE @ValidationErrors NVARCHAR(MAX)
DECLARE @EmploymentTypeGuids AS [dbo].[GuidList]
INSERT INTO @EmploymentTypeGuids VALUES (''FB5F293C-24F3-4473-BF47-EA1F1455272C'')
INSERT INTO @EmploymentTypeGuids VALUES (''C912F22A-1376-482E-8A33-9C2868A4366D'')
EXEC [G2].[System_Update_Profile] @RecruiterSubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @ProfileGuid = ''79F222F2-7994-4A53-97F2-72A92C2A2A2D''
, @CompanyGuid = ''326013DA-E39B-4B8E-A8D9-53FEBC9243D4''
, @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @CityGuid = ''66A83A17-0776-4B19-85FD-B17B06517812''
, @StateGuid = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC''
, @PostalGuid = ''B35BB598-B4B6-4A13-8187-D58491BE811D''
, @ExperienceLevelGuid = ''F3E95E74-1CC4-4547-8F29-11DE077514C1''
, @ContactTypeGuid = ''1C69165E-8005-4E62-8E70-E4EC4EAFCEFB''
, @FirstName = ''Jason''
, @LastName = ''Carr''
, @Email = ''jasonrc42@gmail.com''
, @PhoneNumber = ''4109310684''
, @StreetAddress = ''917 Tartan Hill Dr''
, @Title = ''Electrical Engineer''
, @IsWillingToTravel = 0
, @IsActiveJobSeeker = 0
, @IsCurrentlyEmployed = 1
, @IsWillingToWorkProBono = 0
, @CurrentRate = 45.75
, @DesiredRate = 60.00
, @Goals = ''Testing goals''
, @Preferences = ''Testing preferences''
, @SkillsNote = ''I have skills''
, @EmploymentTypeGuids = @EmploymentTypeGuids
, @ValidationErrors = @ValidationErrors OUTPUT
SELECT @ValidationErrors ValidationErrors
</example>
*/
ALTER PROCEDURE [G2].[System_Update_Profile] (
    @RecruiterSubscriberGuid UNIQUEIDENTIFIER,
	@ProfileGuid UNIQUEIDENTIFIER, 
    @CompanyGuid UNIQUEIDENTIFIER, 
	@SubscriberGuid UNIQUEIDENTIFIER, 
	@CityGuid UNIQUEIDENTIFIER,
	@StateGuid UNIQUEIDENTIFIER,
	@PostalGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@ContactTypeGuid UNIQUEIDENTIFIER, 
    @FirstName NVARCHAR(100),
	@LastName NVARCHAR(100),
	@Email NVARCHAR(254),
    @PhoneNumber NVARCHAR(20),
    @StreetAddress NVARCHAR(100), 
    @Title NVARCHAR(100),
    @IsWillingToTravel BIT,
    @IsActiveJobSeeker BIT,
    @IsCurrentlyEmployed BIT,
    @IsWillingToWorkProBono BIT,
    @CurrentRate DECIMAL(18,2),
    @DesiredRate DECIMAL(18,2),
    @Goals NVARCHAR(500),
	@Preferences NVARCHAR(500),
    @SkillsNote NVARCHAR(500),
	@EmploymentTypeGuids dbo.GuidList READONLY,
    @ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
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

    -- declare parameters for foreign keys
    DECLARE @CompanyId INT, @SubscriberId INT, @CityId INT, @StateId INT, @PostalId INT, @ExperienceLevelId INT, @ContactTypeId INT = NULL

    SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
    IF(@CompanyId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''
	
    SET @SubscriberId = (SELECT TOP 1 SubscriberId FROM dbo.Subscriber WHERE IsDeleted = 0 AND SubscriberGuid = @SubscriberGuid)
    IF(@SubscriberId IS NULL)
    	SET @ValidationErrors = @ValidationErrors + ''Invalid subscriber specified (required);''

	IF(@CityGuid IS NOT NULL AND @CityGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @CityId = (SELECT TOP 1 CityId FROM dbo.City WHERE IsDeleted = 0 AND CityGuid = @CityGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid city specified;''
	END
	
	IF(@StateGuid IS NOT NULL AND @StateGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @StateId = (SELECT TOP 1 StateId FROM dbo.[State] WHERE IsDeleted = 0 AND StateGuid = @StateGuid)
		IF(@StateId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid state specified;''
	END

	IF(@PostalGuid IS NOT NULL AND @PostalGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @PostalId = (SELECT TOP 1 PostalId FROM dbo.Postal WHERE IsDeleted = 0 AND PostalGuid = @PostalGuid)
		IF(@CityId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid postal code specified;''
	END

	IF(@ExperienceLevelGuid IS NOT NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
		IF(@ExperienceLevelId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
	END

	IF EXISTS(
		SELECT g.[Guid]
		FROM @EmploymentTypeGuids g 
		LEFT JOIN dbo.EmploymentType et ON et.EmploymentTypeGuid = g.[Guid]
		WHERE et.EmploymentTypeGuid IS NULL)
	BEGIN
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type(s) specified;''
	END

	IF(@ContactTypeGuid IS NOT NULL AND @ContactTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @ContactTypeId = (SELECT TOP 1 ContactTypeId FROM G2.ContactTypes WHERE IsDeleted = 0 AND ContactTypeGuid = @ContactTypeGuid)
		IF(@ContactTypeId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''
	END

    -- perform insert if there were no validation errors
    IF(LEN(@ValidationErrors) = 0)
    BEGIN
    	UPDATE G2.Profiles
    	SET ModifyDate = GETUTCDATE()
    		, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
    		, CompanyId = @CompanyId
			, SubscriberId = @SubscriberId
			, CityId = @CityId
			, StateId = @StateId
			, PostalId = @PostalId
			, ExperienceLevelId = @ExperienceLevelId
			, ContactTypeId = @ContactTypeId
			, FirstName = @FirstName
			, LastName = @LastName
			, Email = @Email
			, PhoneNumber = @PhoneNumber
			, StreetAddress = @StreetAddress
			, Title = @Title
			, IsWillingToTravel = @IsWillingToTravel
			, IsActiveJobSeeker = @IsActiveJobSeeker
			, IsCurrentlyEmployed = @IsCurrentlyEmployed
			, IsWillingToWorkProBono = @IsWillingToWorkProBono
			, CurrentRate = @CurrentRate
			, DesiredRate = @DesiredRate
			, Goals = @Goals
			, Preferences = @Preferences
			, SkillsNote = @SkillsNote
			, AzureIndexStatusId = (SELECT TOP 1 AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''Pending'')
			, AzureSearchIndexInfo = NULL
    	WHERE ProfileGuid = @ProfileGuid

		DECLARE @ProfileId INT = (SELECT TOP 1 ProfileId FROM G2.Profiles WHERE ProfileGuid = @ProfileGuid)
		-- saved employment types which do not exist should be created
		;WITH existingEmploymentTypes AS (
			SELECT et.EmploymentTypeId, et.EmploymentTypeGuid
			FROM G2.ProfileEmploymentTypes pet 
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			WHERE pet.ProfileId = @ProfileId
		)
		INSERT INTO G2.ProfileEmploymentTypes (IsDeleted, CreateDate, CreateGuid, ProfileId, EmploymentTypeId, ProfileEmploymentTypeGuid)
		SELECT 0, GETUTCDATE(), ''00000000-0000-0000-0000-000000000000'', @ProfileId, et.EmploymentTypeId, NEWID()
		FROM @EmploymentTypeGuids etg
		INNER JOIN dbo.EmploymentType et ON etg.[Guid] = et.EmploymentTypeGuid
		LEFT JOIN existingEmploymentTypes eet ON etg.[Guid] = eet.EmploymentTypeGuid
		WHERE eet.EmploymentTypeGuid IS NULL

		-- saved employment types which were previously deleted should be made active
		UPDATE 
			pet
		SET
			pet.IsDeleted = 0
			, pet.ModifyDate = GETUTCDATE()
			, pet.ModifyGuid = ''00000000-0000-0000-0000-000000000000''
		FROM  
			G2.ProfileEmploymentTypes pet
			INNER JOIN G2.Profiles p ON pet.ProfileId = p.ProfileId
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			INNER JOIN @EmploymentTypeGuids etg ON et.EmploymentTypeGuid = etg.[Guid]
		WHERE
			p.ProfileId = @ProfileId
			AND pet.IsDeleted = 1

		-- existing employment types not specified in saved employment types should be logically deleted
		UPDATE 
			pet
		SET
			pet.IsDeleted = 1
			, pet.ModifyDate = GETUTCDATE()
			, pet.ModifyGuid = ''00000000-0000-0000-0000-000000000000''
		FROM  
			G2.ProfileEmploymentTypes pet
			INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
			LEFT JOIN @EmploymentTypeGuids etg ON et.EmploymentTypeGuid = etg.[Guid]
		WHERE
			etg.[Guid] IS NULL
			AND pet.ProfileId = @ProfileId
			AND pet.IsDeleted = 0

    END	
    ELSE
    BEGIN
    	-- remove the last occurrence of the semicolon
    	SET @ValidationErrors = CASE WHEN CHARINDEX('';'', @ValidationErrors) = 0 THEN @ValidationErrors ELSE LEFT(@ValidationErrors, LEN(@ValidationErrors) - CHARINDEX('';'', REVERSE(@ValidationErrors) + '';'')) END
    END
END')");

        }
    }
}
