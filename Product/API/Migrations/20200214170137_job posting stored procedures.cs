using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobpostingstoredprocedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.14 - Bill Koenig - Created
</remarks>
<description>
Creates a job posting
</description>
<example>
DECLARE @JobPostingGuid UNIQUEIDENTIFIER, @ValidationErrors NVARCHAR(MAX)
EXEC [dbo].[System_Create_JobPosting] @PostingDateUTC = ''12/31/2020'', @PostingExpirationDateUTC = ''1/31/2021'', @ApplicationDeadlineUTC = ''1/15/2021'', @Title = ''Test''
, @Description = ''This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters.''
, @H2Visa = 0, @IsAgencyJobPosting = 0, @TelecommutePercentage = 0, @Compensation = 0, @ThirdPartyApply = 0, @ThirdPartyApplicationUrl = ''http://www.google.com''
, @Country = ''USA'', @City = ''Baltimore'', @Province = ''MD'', @PostalCode = ''21224'', @StreetAddress = ''1501 S Clinton St'', @IsPrivate = 0, @JobStatus = 0
, @ThirdPartyIdentifier = ''6977842'', @RecruiterGuid = ''00000000-0000-0000-0000-000000000000'', @CompanyGuid = ''507472B4-5302-42F7-9216-EE5E6098915E''
, @IndustryGuid = ''00000000-0000-0000-0000-000000000000'', @JobCategoryGuid = ''00000000-0000-0000-0000-000000000000'', @ExperienceLevelGuid = ''00000000-0000-0000-0000-000000000000''
, @EducationLevelGuid = ''00000000-0000-0000-0000-000000000000'', @CompensationTypeGuid = ''00000000-0000-0000-0000-000000000000'', @SecurityClearanceGuid = ''00000000-0000-0000-0000-000000000000''
, @EmploymentTypeGuid = ''00000000-0000-0000-0000-000000000000'', @JobPostingGuid = @JobPostingGuid OUTPUT, @ValidationErrors = @ValidationErrors OUTPUT
SELECT @JobPostingGuid JobPostingGuid, @ValidationErrors ValidationErrors
</example>
*/
CREATE PROCEDURE [dbo].[System_Create_JobPosting] (
	@PostingDateUTC DATETIME, 
	@PostingExpirationDateUTC DATETIME, 
	@ApplicationDeadlineUTC DATETIME,
	@Title NVARCHAR(MAX), 
	@Description NVARCHAR(MAX),
	@H2Visa BIT,
	@IsAgencyJobPosting BIT,
	@TelecommutePercentage INT,
	@Compensation DECIMAL(18,2),
	@ThirdPartyApply BIT,
	@ThirdPartyApplicationUrl NVARCHAR(MAX),
	@Country NVARCHAR(MAX),
	@City NVARCHAR(MAX),
	@Province NVARCHAR(MAX),
	@PostalCode NVARCHAR(MAX),
	@StreetAddress NVARCHAR(MAX),
	@IsPrivate INT,
	@JobStatus INT,
	@ThirdPartyIdentifier NVARCHAR(MAX),
	@RecruiterGuid UNIQUEIDENTIFIER,
	@CompanyGuid UNIQUEIDENTIFIER,
	@IndustryGuid UNIQUEIDENTIFIER,
	@JobCategoryGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@EducationLevelGuid UNIQUEIDENTIFIER,
	@CompensationTypeGuid UNIQUEIDENTIFIER,
	@SecurityClearanceGuid UNIQUEIDENTIFIER,
	@EmploymentTypeGuid UNIQUEIDENTIFIER,
	@JobPostingGuid UNIQUEIDENTIFIER OUTPUT, 
	@ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
	-- need to set this to be an empty string otherwise concatenation won''t work
	SET @ValidationErrors = ''''

	-- declare parameters for foreign keys
	DECLARE @CompanyId INT, @RecruiterId INT, @IndustryId INT, @JobCategoryId INT, @ExperienceLevelId INT
		, @EducationLevelId INT, @CompensationTypeId INT, @SecurityClearanceId INT, @EmploymentTypeId INT

	SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
	IF(@CompanyId IS NULL)
		SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''

	SET @RecruiterId = (SELECT TOP 1 RecruiterId FROM dbo.Recruiter WHERE IsDeleted = 0 AND RecruiterGuid = @RecruiterGuid)
	IF(@RecruiterId IS NULL AND @RecruiterGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid recruiter specified;''
		
	SET @IndustryId = (SELECT TOP 1 IndustryId FROM dbo.Industry WHERE IsDeleted = 0 AND IndustryGuid = @IndustryGuid)
	IF(@IndustryId IS NULL AND @IndustryGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid industry specified;''
		
	SET @JobCategoryId = (SELECT TOP 1 JobCategoryId FROM dbo.JobCategory WHERE IsDeleted = 0 AND JobCategoryGuid = @JobCategoryGuid)
	IF(@JobCategoryId IS NULL AND @JobCategoryGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid job category specified;''
		
	SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
	IF(@ExperienceLevelId IS NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
		
	SET @EducationLevelId = (SELECT TOP 1 EducationLevelId FROM dbo.EducationLevel WHERE IsDeleted = 0 AND EducationLevelGuid = @EducationLevelGuid)
	IF(@EducationLevelId IS NULL AND @EducationLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid education level specified;''
		
	SET @CompensationTypeId = (SELECT TOP 1 CompensationTypeId FROM dbo.CompensationType WHERE IsDeleted = 0 AND CompensationTypeGuid = @CompensationTypeGuid)
	IF(@CompensationTypeId IS NULL AND @CompensationTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid compensation type specified;''
		
	SET @SecurityClearanceId = (SELECT TOP 1 SecurityClearanceId FROM dbo.SecurityClearance WHERE IsDeleted = 0 AND SecurityClearanceGuid = @SecurityClearanceGuid)
	IF(@SecurityClearanceId IS NULL AND @SecurityClearanceGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid security clearance specified;''
		
	SET @EmploymentTypeId = (SELECT TOP 1 EmploymentTypeId FROM dbo.EmploymentType WHERE IsDeleted = 0 AND EmploymentTypeGuid = @EmploymentTypeGuid)
	IF(@EmploymentTypeId IS NULL AND @EmploymentTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''

	IF(LEN(@Description) < 300)
		SET @ValidationErrors = @ValidationErrors + ''Description must be at least 300 characters in length;''
		
	-- perform insert if there were no validation errors
	IF(LEN(@ValidationErrors) = 0)
	BEGIN
		SET @JobPostingGuid = NEWID()
		INSERT INTO dbo.JobPosting (IsDeleted
			, CreateDate
			, CreateGuid
			, JobPostingGuid
			, PostingDateUTC
			, PostingExpirationDateUTC
			, CloudTalentIndexStatus
			, IndustryId
			, Title
			, [Description]
			, SecurityClearanceId
			, H2Visa
			, TelecommutePercentage
			, Compensation
			, CompensationTypeId
			, ThirdPartyApply
			, ThirdPartyApplicationUrl
			, StreetAddress
			, CompanyId
			, EmploymentTypeId
			, ApplicationDeadlineUTC
			, ExperienceLevelId
			, EducationLevelId
			, IsAgencyJobPosting
			, JobCategoryId
			, RecruiterId
			, JobStatus
			, City
			, Country
			, PostalCode
			, Province
			, ThirdPartyIdentifier
			, IsPrivate)
		VALUES (0
			, GETUTCDATE()
			, ''00000000-0000-0000-0000-000000000000''
			, @JobPostingGuid
			, @PostingDateUTC
			, @PostingExpirationDateUTC
			, 0
			, @IndustryId
			, @Title
			, @Description
			, @SecurityClearanceId
			, @H2Visa
			, @TelecommutePercentage
			, @Compensation
			, @CompensationTypeId
			, @ThirdPartyApply
			, @ThirdPartyApplicationUrl
			, @StreetAddress
			, @CompanyId
			, @EmploymentTypeId
			, @ApplicationDeadlineUTC
			, @ExperienceLevelId
			, @EducationLevelId
			, @IsAgencyJobPosting
			, @JobCategoryId
			, @RecruiterId
			, @JobStatus
			, @City
			, @Country
			, @PostalCode
			, @Province
			, @ThirdPartyIdentifier
			, @IsPrivate)
	END
	ELSE
	BEGIN
		-- remove the last occurrence of the semicolon
		SET @ValidationErrors = CASE WHEN CHARINDEX('';'', @ValidationErrors) = 0 THEN @ValidationErrors ELSE LEFT(@ValidationErrors, LEN(@ValidationErrors) - CHARINDEX('';'', REVERSE(@ValidationErrors) + '';'')) END
	END
END')");
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.14 - Bill Koenig - Created
</remarks>
<description>
Updates a job posting
</description>
<example>
DECLARE @ValidationErrors NVARCHAR(MAX)
EXEC [dbo].[System_Update_JobPosting] @JobPostingGuid = ''6D91ECFB-5E75-4F1B-8CCC-0E47B1552079'', @PostingDateUTC = ''12/31/2022'', @PostingExpirationDateUTC = ''1/31/2022'', @ApplicationDeadlineUTC = ''1/15/2022'', @Title = ''Test update''
, @Description = ''This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters. This is a description that contains the minimum number of characters.''
, @H2Visa = 1, @IsAgencyJobPosting = 1, @TelecommutePercentage = 50, @Compensation = 100000.50, @ThirdPartyApply = 1, @ThirdPartyApplicationUrl = ''http://www.google.com''
, @Country = ''USA'', @City = ''Baltimore'', @Province = ''MD'', @PostalCode = ''21224'', @StreetAddress = ''1501 S Clinton St'', @IsPrivate = 1, @JobStatus = 1
, @ThirdPartyIdentifier = ''6977843'', @RecruiterGuid = ''9DE84B7C-F5F6-4C4F-A8E2-66902EDE24C0'', @CompanyGuid = ''507472B4-5302-42F7-9216-EE5E6098915E''
, @IndustryGuid = ''AABD39CE-40A8-410F-9A53-A3CD866CF8A3'', @JobCategoryGuid = ''7B25FC3F-2BF1-4835-9F67-BD9F082A8886'', @ExperienceLevelGuid = ''F105B4E4-643B-4BEB-BDF3-9595508C5620''
, @EducationLevelGuid = ''07A97D32-6E17-42E7-BDA2-8CDC07B6CFF1'', @CompensationTypeGuid = ''C3D927D9-BFBB-4205-B9BA-3956DAB26F0B'', @SecurityClearanceGuid = ''5F57CFA2-F4AA-4E50-AA4F-8060B5B4161F''
, @EmploymentTypeGuid = ''EB3F2DB6-AAA9-4660-A92D-A11E4D83F23A'', @ValidationErrors = @ValidationErrors OUTPUT
SELECT @ValidationErrors ValidationErrors
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_JobPosting] (
	@JobPostingGuid UNIQUEIDENTIFIER, 
	@PostingDateUTC DATETIME, 
	@PostingExpirationDateUTC DATETIME,
	@ApplicationDeadlineUTC DATETIME,
	@Title NVARCHAR(MAX), 
	@Description NVARCHAR(MAX),
	@H2Visa BIT,
	@IsAgencyJobPosting BIT,
	@TelecommutePercentage INT,
	@Compensation DECIMAL(18,2),
	@ThirdPartyApply BIT,
	@ThirdPartyApplicationUrl NVARCHAR(MAX),
	@Country NVARCHAR(MAX),
	@City NVARCHAR(MAX),
	@Province NVARCHAR(MAX),
	@PostalCode NVARCHAR(MAX),
	@StreetAddress NVARCHAR(MAX),
	@IsPrivate INT,
	@JobStatus INT,
	@ThirdPartyIdentifier NVARCHAR(MAX),
	@RecruiterGuid UNIQUEIDENTIFIER,
	@CompanyGuid UNIQUEIDENTIFIER,
	@IndustryGuid UNIQUEIDENTIFIER,
	@JobCategoryGuid UNIQUEIDENTIFIER,
	@ExperienceLevelGuid UNIQUEIDENTIFIER,
	@EducationLevelGuid UNIQUEIDENTIFIER,
	@CompensationTypeGuid UNIQUEIDENTIFIER,
	@SecurityClearanceGuid UNIQUEIDENTIFIER,
	@EmploymentTypeGuid UNIQUEIDENTIFIER,
	@ValidationErrors NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
	-- need to set this to be an empty string otherwise concatenation won''t work
	SET @ValidationErrors = ''''

	-- declare parameters for foreign keys
	DECLARE @CompanyId INT, @RecruiterId INT, @IndustryId INT, @JobCategoryId INT, @ExperienceLevelId INT
		, @EducationLevelId INT, @CompensationTypeId INT, @SecurityClearanceId INT, @EmploymentTypeId INT

	SET @CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyGuid = @CompanyGuid)
	IF(@CompanyId IS NULL)
		SET @ValidationErrors = @ValidationErrors + ''Invalid company specified (required);''

	SET @RecruiterId = (SELECT TOP 1 RecruiterId FROM dbo.Recruiter WHERE IsDeleted = 0 AND RecruiterGuid = @RecruiterGuid)
	IF(@RecruiterId IS NULL AND @RecruiterGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid recruiter specified;''
		
	SET @IndustryId = (SELECT TOP 1 IndustryId FROM dbo.Industry WHERE IsDeleted = 0 AND IndustryGuid = @IndustryGuid)
	IF(@IndustryId IS NULL AND @IndustryGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid industry specified;''
		
	SET @JobCategoryId = (SELECT TOP 1 JobCategoryId FROM dbo.JobCategory WHERE IsDeleted = 0 AND JobCategoryGuid = @JobCategoryGuid)
	IF(@JobCategoryId IS NULL AND @JobCategoryGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid job category specified;''
		
	SET @ExperienceLevelId = (SELECT TOP 1 ExperienceLevelId FROM dbo.ExperienceLevel WHERE IsDeleted = 0 AND ExperienceLevelGuid = @ExperienceLevelGuid)
	IF(@ExperienceLevelId IS NULL AND @ExperienceLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid experience level specified;''
		
	SET @EducationLevelId = (SELECT TOP 1 EducationLevelId FROM dbo.EducationLevel WHERE IsDeleted = 0 AND EducationLevelGuid = @EducationLevelGuid)
	IF(@EducationLevelId IS NULL AND @EducationLevelGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid education level specified;''
		
	SET @CompensationTypeId = (SELECT TOP 1 CompensationTypeId FROM dbo.CompensationType WHERE IsDeleted = 0 AND CompensationTypeGuid = @CompensationTypeGuid)
	IF(@CompensationTypeId IS NULL AND @CompensationTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid compensation type specified;''
		
	SET @SecurityClearanceId = (SELECT TOP 1 SecurityClearanceId FROM dbo.SecurityClearance WHERE IsDeleted = 0 AND SecurityClearanceGuid = @SecurityClearanceGuid)
	IF(@SecurityClearanceId IS NULL AND @SecurityClearanceGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid security clearance specified;''
		
	SET @EmploymentTypeId = (SELECT TOP 1 EmploymentTypeId FROM dbo.EmploymentType WHERE IsDeleted = 0 AND EmploymentTypeGuid = @EmploymentTypeGuid)
	IF(@EmploymentTypeId IS NULL AND @EmploymentTypeGuid <> ''00000000-0000-0000-0000-000000000000'')
		SET @ValidationErrors = @ValidationErrors + ''Invalid employment type specified;''

	IF(LEN(@Description) < 300)
		SET @ValidationErrors = @ValidationErrors + ''Description must be at least 300 characters in length;''

	IF ((SELECT COUNT(1) FROM dbo.JobPosting WHERE JobPostingGuid = @JobPostingGuid) <> 1)
		SET @ValidationErrors = @ValidationErrors + ''A unique job was not found for the provided identifier;''

	-- perform insert if there were no validation errors
	IF(LEN(@ValidationErrors) = 0)
	BEGIN
		UPDATE dbo.JobPosting
		SET ModifyDate = GETUTCDATE()
			, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
			, PostingDateUTC = @PostingDateUTC
			, PostingExpirationDateUTC = @PostingExpirationDateUTC
			, IndustryId = @IndustryId
			, Title = @Title
			, [Description] = @Description
			, SecurityClearanceId = @SecurityClearanceId
			, H2Visa = @H2Visa
			, TelecommutePercentage = @TelecommutePercentage
			, Compensation = @Compensation
			, CompensationTypeId = @CompensationTypeId
			, ThirdPartyApply = @ThirdPartyApply
			, ThirdPartyApplicationUrl = @ThirdPartyApplicationUrl
			, StreetAddress = @StreetAddress
			, CompanyId = @CompanyId
			, EmploymentTypeId = @EmploymentTypeId
			, ApplicationDeadlineUTC = @ApplicationDeadlineUTC
			, ExperienceLevelId = @ExperienceLevelId
			, EducationLevelId = @EducationLevelId
			, IsAgencyJobPosting = @IsAgencyJobPosting
			, JobCategoryId = @JobCategoryId
			, RecruiterId = @RecruiterId
			, JobStatus = @JobStatus
			, City = @City
			, Country = @Country
			, PostalCode = @PostalCode
			, Province = @Province
			, ThirdPartyIdentifier = @ThirdPartyIdentifier
			, IsPrivate = @IsPrivate
		WHERE JobPostingGuid = @JobPostingGuid
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

        }
    }
}
