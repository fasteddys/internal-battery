using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class azureindexinfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AzureSearchIndexInfo",
                schema: "G2",
                table: "Profiles",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.03 - Bill Koenig - Updates AzureSearchIndexInfo to be NULL (since we are setting the status to pending, it wouldn''t make sense to have the previous operation''s info here)
</remarks>
<description>
Updates a profile
</description>
<example>
DECLARE @ValidationErrors NVARCHAR(MAX)
EXEC [G2].[System_Update_Profile] @RecruiterSubscriberGuid = ''2CC598FE-03B3-4528-A96F-C9452F4B1C58''
, @ProfileGuid = ''BDBD4476-8006-44EC-B21A-1D2D9C834EA1''
, @CompanyGuid = ''C8614494-7D20-459C-BBD1-DA87F9D4B5F6''
, @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB''
, @CityGuid = ''66A83A17-0776-4B19-85FD-B17B06517812''
, @StateGuid = ''8823F7F7-ECC2-4FE3-8461-79FDCB6873CC''
, @PostalGuid = ''B35BB598-B4B6-4A13-8187-D58491BE811D''
, @ExperienceLevelGuid = ''F3E95E74-1CC4-4547-8F29-11DE077514C1''
, @EmploymentTypeGuid = ''EB3F2DB6-AAA9-4660-A92D-A11E4D83F23A''
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
	@EmploymentTypeGuid UNIQUEIDENTIFIER,
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
		INNER JOIN Recruiter r ON c.CompanyId = r.CompanyId
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
		WHERE p.ProfileGuid = @ProfileGuid
		AND s.SubscriberGuid = @RecruiterSubscriberGuid
		AND s.IsDeleted = 0
		AND p.IsDeleted = 0)
		SET @ValidationErrors = @ValidationErrors + ''The specified recruiter does not have permission to edit this profile;''

    -- declare parameters for foreign keys
    DECLARE @CompanyId INT, @SubscriberId INT, @CityId INT, @StateId INT, @PostalId INT, @ExperienceLevelId INT, @EmploymentTypeId INT, @ContactTypeId INT = NULL

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

	IF(@EmploymentTypeGuid IS NOT NULL AND @EmploymentTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		SET @EmploymentTypeId = (SELECT TOP 1 EmploymentTypeId FROM dbo.EmploymentType WHERE IsDeleted = 0 AND EmploymentTypeGuid = @EmploymentTypeGuid)
		IF(@EmploymentTypeId IS NULL)
			SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''
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
			, EmploymentTypeId = @EmploymentTypeId
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
			, AzureIndexStatusId = (SELECT TOP 1 AzureIndexStatusId FROM G2.AzureIndexStatuses WHERE [Name] = ''Pending'')
			, AzureSearchIndexInfo = NULL
    	WHERE ProfileGuid = @ProfileGuid
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
            migrationBuilder.AlterColumn<string>(
                name: "AzureSearchIndexInfo",
                schema: "G2",
                table: "Profiles",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
