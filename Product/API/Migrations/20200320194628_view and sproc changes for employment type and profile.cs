using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class viewandsprocchangesforemploymenttypeandprofile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.09 - Jab - Created
2020.03.11 - Jab - Modified to only unclude companies with active subscribers
2020.03.17 - Jab Fixed bug with undeleting deleted profiles.  Also fixed issue with finding companies with active recruiters
2020.03.18 - Jab - Modified to default IsWillingtoTravel, IsActiveJobSeeker,IsCurrentlyEmployed and IsWillingToWorkProBono to null
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
</remarks>
<description>
Adds a subcriber g2 profile record for every active subscriber for every active company
</description>
<example>
EXEC [G2].[System_Create_G2Profiles] @SubscriberGuid = ''''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''''
</example>
*/
CREATE PROCEDURE [G2].[System_Create_G2Profiles]  
AS
BEGIN 
 
   -- Add new G2s 
	;With ProfilesToInsert AS
(
	SELECT 
	0 as IsDeleted
	,getutcdate() as CreateDate
	,null as Modifydate
	,''00000000-0000-0000-0000-000000000000'' as CreateGuid
	,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
	,newid() as ProfileGuid
	,c.Companyid
	,s.subscriberid
	,s.FirstName
	,s.Email
	,s.PhoneNumber
	,null as ContactTypeId
	,s.Address as StreetAddress
	,null as CityId
	,s.StateId
	,null as PostalId
	,null as ExperienceLevelId
	,s.Title
	,null as IsWillingToTravel
	,null as IsActivejobSeeker
	,null as IsCurrentlyEmployed
	,null as IsWillingToWorkProBono
	,0 as CurrentRate
	,0 as DesiredRate
	,null as Goals
	,null as Preferences
	,1 as AzureIndexStatusId
FROM Subscriber  s
LEFT JOIN Company c on c.IsDeleted = 0 and c.CompanyName != ''public data'' and c.CompanyId in 
(
select c.companyId 
from recruiter r
inner join RecruiterCompany rc on r.RecruiterId = rc.RecruiterId
inner join Company c on rc.CompanyId = c.CompanyId
inner join Subscriber s on r.SubscriberId = s.SubscriberId
where s.IsDeleted = 0 
)
Where s.IsDeleted = 0 and  not exists ( select * from g2.Profiles where CompanyId  = c.CompanyId and SubscriberId  = s.SubscriberId )
)
INSERT INTO g2.Profiles   
(
    IsDeleted
   ,CreateDate
   ,Modifydate
   ,CreateGuid
   ,ModifyGuid
   ,ProfileGuid
   ,Companyid
   ,Subscriberid
   ,FirstName
   ,Email
   ,PhoneNumber
   ,ContactTypeId
   ,StreetAddress
   ,CityId
   ,StateId
   ,PostalId
   ,ExperienceLevelId
   ,Title
   ,IsWillingToTravel
   ,IsActivejobSeeker
   ,IsCurrentlyEmployed
   ,IsWillingToWorkProBono
   ,CurrentRate
   ,DesiredRate
   ,Goals
   ,Preferences
   ,AzureIndexStatusId
)
Select * from ProfilesToInsert

return  @@ROWCOUNT
END')");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Create_G2Profiles]");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.28 - Jab - Created
2020.03.02 - JAB - Modified to undelete user profile records before adding new profile records.  Also modified to return rows affected from both the update and insert statements
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
</remarks>
<description>
Adds a subcriber g2 profile record for a new subscriber.  1 record per active company is created 
</description>
<example>
EXEC [G2].[System_Create_SubscriberG2Profiles] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [G2].[System_Create_SubscriberG2Profiles] (
	@SubscriberGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 


    DECLARE @RowsAffected INT;

   --- First restore any previously deleted profiles for the user 
    Update G2.Profiles SET IsDeleted = 0  
	WHERE IsDeleted = 1 and SubscriberId = (  SELECT SubscriberId FROM Subscriber WHERE SubscriberGuid = @SubscriberGuid )

	Set @RowsAffected = @@RowCount;

   -- Add new G2s 

	;With ProfilesToInsert AS
(
	SELECT 
	0 as IsDeleted
	,getutcdate() as CreateDate
	,null as Modifydate
	,''00000000-0000-0000-0000-000000000000'' as CreateGuid
	,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
	,newid() as ProfileGuid
	,c.Companyid
	,s.subscriberid
	,s.FirstName
	,s.Email
	,s.PhoneNumber
	,null as ContactTypeId
	,s.Address as StreetAddress
	,null as CityId
	,s.StateId
	,null as PostalId
	,null as ExperienceLevelId
	,s.Title
	,0 as IsWillingToTravel
	,0 as IsActivejobSeeker
	,0 as IsCurrentlyEmployed
	,0 as IsWillingToWorkProBono
	,0 as CurrentRate
	,0 as DesiredRate
	,null as Goals
	,null as Preferences
FROM Company c
LEFT JOIN subscriber s on s.SubscriberGuid = @SubscriberGuid
WHERE C.CompanyId NOT IN ( Select CompanyId from g2.Profiles where SubscriberGuid = @SubscriberGuid) and c.IsDeleted = 0
)
INSERT INTO g2.Profiles   
(
    IsDeleted
   ,CreateDate
   ,Modifydate
   ,CreateGuid
   ,ModifyGuid
   ,ProfileGuid
   ,Companyid
   ,Subscriberid
   ,FirstName
   ,Email
   ,PhoneNumber
   ,ContactTypeId
   ,StreetAddress
   ,CityId
   ,StateId
   ,PostalId
   ,ExperienceLevelId
   ,Title
   ,IsWillingToTravel
   ,IsActivejobSeeker
   ,IsCurrentlyEmployed
   ,IsWillingToWorkProBono
   ,CurrentRate
   ,DesiredRate
   ,Goals
   ,Preferences
)
Select * from ProfilesToInsert

Set @RowsAffected = @RowsAffected + @@ROWCOUNT	 

return @RowsAffected
END')");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Create_SubscriberG2Profiles]");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.03 - Jab - Created
2020.03.09 - Jab - Fixed bug with ProfilesToInsert CTE.  The where condition for determining which subscribers already had a profile for the specified 
         company was wrong
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many), changed schema name to G2
</remarks>
<description>
Adds a subcriber g2 profile record for a new company.  1 record per active subscriber is created 
</description>
<example>
EXEC [G2].[System_Create_SubscriberG2Profiles] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [G2].[System_Create_CompanyG2Profiles] (
	@CompanyGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 


    DECLARE @RowsAffected INT;

   --- First restore any previously deleted profiles for the user 
    Update G2.Profiles SET IsDeleted = 0  
	WHERE IsDeleted = 1 and CompanyId = (  SELECT CompanyId FROM Company WHERE CompanyGuid = @CompanyGuid )

	Set @RowsAffected = @@RowCount;

   -- Add new G2s 

	;With ProfilesToInsert AS
(
	SELECT 
	0 as IsDeleted
	,getutcdate() as CreateDate
	,null as Modifydate
	,''00000000-0000-0000-0000-000000000000'' as CreateGuid
	,''00000000-0000-0000-0000-000000000000'' as ModifyGuid
	,newid() as ProfileGuid
	,c.Companyid
	,s.subscriberid
	,s.FirstName
	,s.Email
	,s.PhoneNumber
	,null as ContactTypeId
	,s.Address as StreetAddress
	,null as CityId
	,s.StateId
	,null as PostalId
	,null as ExperienceLevelId
	,s.Title
	,0 as IsWillingToTravel
	,0 as IsActivejobSeeker
	,0 as IsCurrentlyEmployed
	,0 as IsWillingToWorkProBono
	,0 as CurrentRate
	,0 as DesiredRate
	,null as Goals
	,null as Preferences
FROM Subscriber  s
LEFT JOIN Company c on c.CompanyGuid = @CompanyGuid
WHERE s.SubscriberId NOT IN ( Select SubscriberId from g2.Profiles p where p.CompanyId = (select companyid from company where CompanyGuid = @CompanyGuid) ) and s.IsDeleted = 0
)
INSERT INTO g2.Profiles   
(
    IsDeleted
   ,CreateDate
   ,Modifydate
   ,CreateGuid
   ,ModifyGuid
   ,ProfileGuid
   ,Companyid
   ,Subscriberid
   ,FirstName
   ,Email
   ,PhoneNumber
   ,ContactTypeId
   ,StreetAddress
   ,CityId
   ,StateId
   ,PostalId
   ,ExperienceLevelId
   ,Title
   ,IsWillingToTravel
   ,IsActivejobSeeker
   ,IsCurrentlyEmployed
   ,IsWillingToWorkProBono
   ,CurrentRate
   ,DesiredRate
   ,Goals
   ,Preferences
)
Select * from ProfilesToInsert

Set @RowsAffected = @RowsAffected + @@ROWCOUNT	 

return @RowsAffected
END')");
            migrationBuilder.Sql("DROP PROCEDURE[dbo].[System_Create_CompanyG2Profiles]");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - Bill Koenig - Created
2020.03.20 - Bill Koenig - Updated relationship between profile and employment type (1:many)
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
, @EmploymentTypeGuids = @EmploymentTypeGuids
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

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-02-21 - Bill Koenig - Created
2020-02-25 - Bill Koenig - Changed data type for current and desired rate, removed support for one to many search locations, added single location	column which returns ''geography'' data type, prioritized the selection of the geo data based on specificity (function), incorporated public and private profile into CTEs, included modify date that indicates the most recent change across all profiles for indexing operations, added code comments for readability
2020-02-26 - Bill Koenig - Primary CTE is now subscriberProfiles with LEFT JOINs to publicProfiles and companyProfiles so that every subscriber has a record whether or not they have a public profile (we still need to create companyProfiles for everyone though), added SubscriberGuid and CompanyGuid to the view output, added check for logical deletes on profiles
2020-03-10 - JAB - Added AzureIndexStatus 
2020-03-20 - Bill Koenig - Added support for multiple employment types for each profile
</remarks>
<description>
Returns G2 profile data from multiple sources in a flattened format for indexing in Azure Search.
</description>
<example>
SELECT * FROM [G2].[v_ProfileAzureSearch]
</example>
*/
ALTER VIEW [G2].[v_ProfileAzureSearch]
AS
	WITH subscriberWorkHistoryWithRownum AS (
		-- returns all subscriber work history with compensation data if it is hourly
		SELECT swh.SubscriberId, swh.StartDate, swh.Title, swh.Compensation, ct.CompensationTypeName, ISNULL(swh.ModifyDate, swh.CreateDate) ModifyDate, ROW_NUMBER() OVER(PARTITION BY swh.SubscriberId ORDER BY swh.StartDate DESC) rownum
		FROM SubscriberWorkHistory swh
		LEFT JOIN CompensationType ct ON swh.CompensationTypeId = ct.CompensationTypeId
		WHERE swh.IsDeleted = 0
	), mostRecentSubscriberWorkHistory AS (
		-- uses the previous CTE to find the most recent work history record to use for title and compensation in subscriber curated data
		SELECT SubscriberId, Title, Compensation, CompensationTypeName, ModifyDate
		FROM subscriberWorkHistoryWithRownum
		WHERE rownum = 1
	), subscriberSkills AS (
		-- returns subscriber curated skill data
		SELECT ss.SubscriberId, s.SkillName, ISNULL(ss.ModifyDate, ss.CreateDate) ModifyDate
		FROM SubscriberSkill ss
		INNER JOIN Skill s ON ss.SkillId = s.SkillId
		WHERE ss.IsDeleted = 0
		AND s.IsDeleted = 0
	), subscriberProfiles AS (
		-- returns a flattened view of all subscriber curated data
		SELECT s.SubscriberId, s.SubscriberGuid, CASE WHEN LEN(FirstName) > 0 THEN FirstName ELSE NULL END FirstName, CASE WHEN LEN(LastName) > 0 THEN LastName ELSE NULL END LastName, Email, CASE WHEN LEN(PhoneNumber) > 0 THEN PhoneNumber ELSE NULL END PhoneNumber, CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress, CASE WHEN LEN(City) > 0 THEN City ELSE NULL END City, t.Code [State], CASE WHEN LEN(PostalCode) > 0 THEN PostalCode ELSE NULL END Postal, mrswh.Title, CASE WHEN mrswh.CompensationTypeName = ''Hourly'' THEN mrswh.Compensation ELSE NULL END CurrentRate, STRING_AGG(ss.SkillName, '';'') SubscriberSkills
		, dbo.fn_GetGeographyCoordinate((SELECT TOP 1 CityId FROM dbo.City c INNER JOIN dbo.[State] t ON c.StateId = t.StateId WHERE c.[Name] = s.City AND t.StateId = s.StateId), s.StateId, (SELECT TOP 1 PostalId FROM dbo.Postal WHERE Code = s.PostalCode)) [Location]
		, dbo.fn_GreatestDate(ISNULL(s.ModifyDate, s.CreateDate), mrswh.ModifyDate, MAX(ss.ModifyDate), default, default) ModifyDate
		FROM Subscriber s	
		LEFT JOIN [State] t ON s.StateId = t.StateId
		LEFT JOIN mostRecentSubscriberWorkHistory mrswh ON s.SubscriberId = mrswh.SubscriberId
		LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId
		WHERE s.IsDeleted = 0
		GROUP BY s.SubscriberId, s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.[Address], s.City, t.Code, s.PostalCode, mrswh.Title, mrswh.CompensationTypeName, mrswh.Compensation, s.StateId, s.CreateDate, s.ModifyDate, mrswh.ModifyDate
	), profileTags AS (
		-- flattens tags that have been applied to a profile
		SELECT p.ProfileId, STRING_AGG(t.[Name], '';'') Tags, dbo.fn_GreatestDate(MAX(pt.CreateDate), MAX(pt.ModifyDate), default, default, default) ModifyDate
		FROM G2.Profiles p
		LEFT JOIN G2.ProfileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN dbo.Tag t ON pt.TagId = t.TagId
		WHERE pt.IsDeleted = 0
		GROUP BY p.ProfileId
	), profileSkills AS (
		-- flattens skills that have been applied to a profile
		SELECT p.ProfileId, STRING_AGG(s.SkillName, '';'') Skills, dbo.fn_GreatestDate(MAX(ps.CreateDate), MAX(ps.ModifyDate), default, default, default) ModifyDate
		FROM G2.Profiles p
		LEFT JOIN G2.ProfileSkills ps ON p.ProfileId = ps.ProfileId
		LEFT JOIN dbo.Skill s ON s.SkillId = ps.SkillId
		WHERE ps.IsDeleted = 0
		GROUP BY p.ProfileId
	), profileEmploymentTypes AS (
		-- flattens employment types that have been applied to a profile
		SELECT p.ProfileId, STRING_AGG(et.[Name], '';'') EmploymentTypes, dbo.fn_GreatestDate(MAX(pet.CreateDate), MAX(pet.ModifyDate), default, default, default) ModifyDate
		FROM G2.Profiles p
		LEFT JOIN G2.ProfileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
		LEFT JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
		WHERE pet.IsDeleted = 0
		GROUP BY p.ProfileId		
	), publicProfiles AS (
		-- public profiles contains the ''base'' profile image as well as data extracted from third party sources such as Connected
		SELECT p.ProfileGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, ps.Skills PublicSkills, NULL PrivateSkills, pet.EmploymentTypes
		, dbo.fn_GetGeographyCoordinate(p.CityId, p.StateId, p.PostalId) [Location]
		, dbo.fn_GreatestDate(ISNULL(p.ModifyDate, p.CreateDate), pt.ModifyDate, ps.ModifyDate, default, default) ModifyDate
		FROM G2.Profiles p
		INNER JOIN Subscriber su ON p.SubscriberId = su.SubscriberId
		INNER JOIN dbo.Company co ON p.CompanyId = co.CompanyId
		LEFT JOIN G2.ContactTypes ct ON p.ContactTypeId = ct.ContactTypeId
		LEFT JOIN City c ON p.CityId = c.CityId
		LEFT JOIN [State] s ON p.StateId = s.StateId
		LEFT JOIN Postal z ON p.PostalId = z.PostalId
		LEFT JOIN ExperienceLevel el ON p.ExperienceLevelId = el.ExperienceLevelId
		LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
		LEFT JOIN profileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
		WHERE co.CompanyName = ''Public Data''
        AND p.IsDeleted = 0
	), companyProfiles AS (
		-- company profiles contains the company-specific data that is populated by recruiters of that company
		SELECT p.ProfileGuid, co.CompanyGuid, su.SubscriberId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, ct.[Name] ContactType, p.StreetAddress, c.[Name] City, s.Code [State], z.Code Postal, el.DisplayName ExperienceLevel, p.Title, p.IsWillingToTravel, p.IsActiveJobSeeker, p.IsCurrentlyEmployed, p.IsWillingToWorkProBono, CAST(p.CurrentRate AS FLOAT) CurrentRate, CAST(p.DesiredRate AS FLOAT) DesiredRate, pt.Tags, NULL PublicSkills, ps.Skills PrivateSkills, pet.EmploymentTypes
		, dbo.fn_GetGeographyCoordinate(p.CityId, p.StateId, p.PostalId) [Location]
		, dbo.fn_GreatestDate(ISNULL(p.ModifyDate, p.CreateDate), pt.ModifyDate, ps.ModifyDate, default, default) ModifyDate, p.AzureIndexStatusId
		FROM G2.Profiles p
		INNER JOIN Subscriber su ON p.SubscriberId = su.SubscriberId
		INNER JOIN dbo.Company co ON p.CompanyId = co.CompanyId
		LEFT JOIN G2.ContactTypes ct ON p.ContactTypeId = ct.ContactTypeId
		LEFT JOIN City c ON p.CityId = c.CityId
		LEFT JOIN [State] s ON p.StateId = s.StateId
		LEFT JOIN Postal z ON p.PostalId = z.PostalId
		LEFT JOIN ExperienceLevel el ON p.ExperienceLevelId = el.ExperienceLevelId
		LEFT JOIN profileTags pt ON p.ProfileId = pt.ProfileId
		LEFT JOIN profileSkills ps ON p.ProfileId = ps.ProfileId
		LEFT JOIN profileEmploymentTypes pet ON p.ProfileId = pet.ProfileId
		WHERE co.CompanyName <> ''Public Data''
        AND p.IsDeleted = 0
	)
	-- subscriber data is prioritized in the following way: company specific first (if it exists), then public data second (if that exists), and finally subscriber-curated data
	SELECT sp.SubscriberGuid, cp.ProfileGuid, cp.CompanyGuid
		, COALESCE(cp.FirstName, pp.FirstName, sp.FirstName) FirstName
		, COALESCE(cp.LastName, pp.LastName, sp.LastName) LastName
		, COALESCE(cp.Email, pp.Email, sp.Email) Email
		, COALESCE(cp.PhoneNumber, pp.PhoneNumber, sp.PhoneNumber) PhoneNumber
		, COALESCE(cp.ContactType, pp.ContactType) ContactType
		, COALESCE(cp.StreetAddress, pp.StreetAddress, sp.StreetAddress) StreetAddress
		, COALESCE(cp.City, pp.City, sp.City) City
		, COALESCE(cp.[State], pp.[State], sp.[State]) [State]
		, COALESCE(cp.Postal, pp.Postal, sp.Postal) Postal 
		, COALESCE(cp.ExperienceLevel, pp.ExperienceLevel) ExperienceLevel 
		, COALESCE(cp.EmploymentTypes, pp.EmploymentTypes) EmploymentTypes
		, COALESCE(cp.Title, pp.Title, sp.Title) Title
		, COALESCE(cp.IsWillingToTravel, pp.IsWillingToTravel) IsWillingToTravel
		, COALESCE(cp.IsActiveJobSeeker, pp.IsActiveJobSeeker) IsActiveJobSeeker
		, COALESCE(cp.IsCurrentlyEmployed , pp.IsCurrentlyEmployed) IsCurrentlyEmployed
		, COALESCE(cp.IsWillingToWorkProBono, pp.IsWillingToWorkProBono) IsWillingToWorkProBono
		, COALESCE(cp.CurrentRate, pp.CurrentRate, sp.CurrentRate) CurrentRate
		, COALESCE(cp.DesiredRate, pp.DesiredRate) DesiredRate
		, COALESCE(cp.Tags, pp.Tags) Tags
		, COALESCE(pp.PublicSkills, sp.SubscriberSkills) PublicSkills
		, cp.PrivateSkills
	  , COALESCE(cp.[Location], pp.[Location], sp.[Location]) [Location]
	  , dbo.fn_GreatestDate(cp.ModifyDate, pp.ModifyDate, sp.ModifyDate, default, default) ModifyDate
	  , cp.AzureIndexStatusId AzureIndexStatusId
	  
	FROM subscriberProfiles sp
	LEFT JOIN publicProfiles pp ON sp.SubscriberId = pp.SubscriberId
	LEFT JOIN companyProfiles cp ON sp.SubscriberId = cp.SubscriberId')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.02 - JAB - Created
2020.03.20 - Bill Koenig - Updated schema name to G2
</remarks>
<description>
Deletes all subcriber g2 profile records for the given subscriber.  
</description>
<example>
EXEC [G2].[System_Delete_SubscriberG2Profiles] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [G2].[System_Delete_SubscriberG2Profiles] (
	@SubscriberGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 

    SELECT * 
	INTO #ProfileIDs 
	FROM
	(
	   SELECT p.ProfileId 
	   FROM G2.Profiles p 
       JOIN Subscriber s ON p.SubscriberId = s.SubscriberId	   	   
	   WHERE s.SubscriberGuid = @SubscriberGuid
	) AS profiles

	--- Delete from Profile Comments 
	UPDATE G2.ProfileComments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Documents 
	UPDATE G2.ProfileDocuments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Search Locations 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Skills 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile Tags 
	Update G2.ProfileTags
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile WishLists  
	Update G2.ProfileWishlists
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profiles 
	Update G2.Profiles
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )  

 
END')");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Delete_SubscriberG2Profiles]");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.03.03 - JAB - Created
2020.03.20 - Bill Koenig - Updated schema name to G2
</remarks>
<description>
Deletes all g2 profile records for the given company.  
</description>
<example>
EXEC [G2].[System_Delete_CompanyG2Profiles] @ComopanyGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [G2].[System_Delete_CompanyG2Profiles] (
	@CompanyGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 

    SELECT * 
	INTO #ProfileIDs 
	FROM
	(
	   SELECT p.ProfileId 
	   FROM G2.Profiles p   	   
	   WHERE p.CompanyId = (SELECT companyid FROM company WHERE companyguid = @CompanyGuid)
	) AS profiles

	--- Delete from Profile Comments 
	UPDATE G2.ProfileComments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Documents 
	UPDATE G2.ProfileDocuments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Search Locations 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Skills 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile Tags 
	Update G2.ProfileTags
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile WishLists  
	Update G2.ProfileWishlists
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profiles 
	Update G2.Profiles
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )  

 
END')");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Delete_CompanyG2Profiles]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
