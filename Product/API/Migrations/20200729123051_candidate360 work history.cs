using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class candidate360workhistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TYPE [dbo].[SubscriberWorkHistory] AS TABLE(	
	StartDate DATETIME NULL,
	CompanyName NVARCHAR(MAX) NULL,
	EndDate DATETIME NULL,
	IsCurrent BIT NULL,
	JobDescription NVARCHAR(MAX) NULL,
	JobTitle NVARCHAR(MAX) NULL,
	SubscriberWorkHistoryGuid UNIQUEIDENTIFIER NULL
)");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.07.26 - Bill Koenig - Created
</remarks>
<description>
Handles updates to a subscriber''s work history.
</description>
<example>
DECLARE @SubscriberWorkHistory AS [dbo].[SubscriberWorkHistory]
INSERT INTO @SubscriberWorkHistory (StartDate, CompanyName, EndDate, IsCurrent, JobDescription, JobTitle, SubscriberWorkHistoryGuid)
VALUES (''1970-01-01'', ''Company Name 1'', ''1981-03-17'', 1, ''Job Description 1'', ''Job Title 1'', NEWID())
INSERT INTO @SubscriberWorkHistory (StartDate, CompanyName, JobDescription, JobTitle, IsCurrent, SubscriberWorkHistoryGuid)
VALUES (''2020-01-01'', ''Chartwell Information Publishers   '', ''Job Description (UPDATE)'', ''Technical Team Lead (UPDATE)'', 1, ''37FF1EEB-614B-404E-B2B1-B91EE3477F56'')
EXEC [dbo].[System_Update_SubscriberWorkHistory] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @SubscriberWorkHistory = @SubscriberWorkHistory
</example>
*/
CREATE PROCEDURE [dbo].[System_Update_SubscriberWorkHistory] (
    @SubscriberGuid UNIQUEIDENTIFIER,
    @SubscriberWorkHistory dbo.SubscriberWorkHistory READONLY
)
AS
BEGIN
	SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
    	-- need this for skill modifications (when no skills exist for a subscriber, cannot use joins based on SubscriberGuid)
    	DECLARE @SubscriberId INT = (SELECT TOP 1 s.SubscriberId FROM Subscriber s WHERE s.SubscriberGuid = @SubscriberGuid)

    	-- replace these with meaningful values (once we have a mechanism in place to generate them)
    	DECLARE @CreateGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''
    	DECLARE @ModifyGuid UNIQUEIDENTIFIER = ''00000000-0000-0000-0000-000000000000''

		-- any company names that do not exist in dbo.Company must be created before moving forward
		INSERT INTO dbo.Company (IsDeleted, CreateDate, CreateGuid, CompanyGuid, CompanyName, CloudTalentIndexStatus, IsHiringAgency, IsJobPoster)
		SELECT 0, GETUTCDATE(), @CreateGuid, NEWID(), swh.CompanyName, 0, 0, 0			
		FROM @SubscriberWorkHistory swh 
		LEFT JOIN dbo.Company c ON TRIM(swh.CompanyName) = c.CompanyName
		WHERE c.CompanyId IS NULL

    	-- work history records which match by subscriberWorkHistoryGuid should be updated
    	UPDATE 
    		eswh
    	SET
			eswh.CompanyId = (SELECT TOP 1 CompanyId FROM dbo.Company WHERE CompanyName = TRIM(swh.CompanyName))
    		, eswh.EndDate = swh.EndDate
			, eswh.IsCurrent = ISNULL(swh.IsCurrent, 0)
			, eswh.IsDeleted = 0
    		, eswh.JobDescription = swh.JobDescription
			, eswh.ModifyDate = GETUTCDATE()
    		, eswh.ModifyGuid = @ModifyGuid
			, eswh.StartDate = swh.StartDate
			, eswh.Title = swh.JobTitle
    	FROM  
    		dbo.SubscriberWorkHistory eswh
    		INNER JOIN Subscriber s ON eswh.SubscriberId = s.SubscriberId
			INNER JOIN @SubscriberWorkHistory swh ON eswh.SubscriberWorkHistoryGuid = swh.SubscriberWorkHistoryGuid
    	WHERE
    		s.SubscriberGuid = @SubscriberGuid

		-- existing work history records with no match in the user-defined table type parameter should be logically deleted
		UPDATE 
    		eswh
    	SET
			eswh.IsDeleted = 1
			, eswh.ModifyDate = GETUTCDATE()
    		, eswh.ModifyGuid = @ModifyGuid
    	FROM  
    		dbo.SubscriberWorkHistory eswh
    		INNER JOIN Subscriber s ON eswh.SubscriberId = s.SubscriberId
			LEFT JOIN @SubscriberWorkHistory swh ON eswh.SubscriberWorkHistoryGuid = swh.SubscriberWorkHistoryGuid
    	WHERE
    		s.SubscriberGuid = @SubscriberGuid
			AND swh.SubscriberWorkHistoryGuid IS NULL
			    	
		-- work history records which do not exist should be created
    	;WITH existingSubscriberWorkHistory AS (
    		SELECT swh.SubscriberWorkHistoryGuid
    		FROM dbo.SubscriberWorkHistory swh 
			LEFT JOIN dbo.Company c ON swh.CompanyId = c.CompanyId
    		INNER JOIN dbo.Subscriber s ON swh.SubscriberId = s.SubscriberId
    		WHERE s.SubscriberGuid = @SubscriberGuid
    	)
    	INSERT INTO dbo.SubscriberWorkHistory (IsDeleted, CreateDate, CreateGuid, SubscriberWorkHistoryGuid, SubscriberId, StartDate, EndDate, IsCurrent, Title, JobDescription, Compensation, CompanyId)
    	SELECT 0, GETUTCDATE(), @CreateGuid, NEWID(), @SubscriberId, swh.StartDate, swh.EndDate, ISNULL(IsCurrent, 0), swh.JobTitle, swh.JobDescription, 0, (SELECT TOP 1 CompanyId FROM dbo.Company WHERE IsDeleted = 0 AND CompanyName = TRIM(swh.CompanyName))
    	FROM @SubscriberWorkHistory swh    	
    	LEFT JOIN existingSubscriberWorkHistory eswh ON eswh.SubscriberWorkHistoryGuid = swh.SubscriberWorkHistoryGuid
    	WHERE eswh.SubscriberWorkHistoryGuid IS NULL

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
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.07.29 - Bill Koenig - Created
</remarks>
<description>
Retrieves a subscriber''s work history.
</description>
<example>
EXEC [dbo].[System_Get_SubscriberWorkHistory] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''isCurrent'', @Order = ''descending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_SubscriberWorkHistory] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT SubscriberWorkHistoryId
		FROM dbo.SubscriberWorkHistory swh
		INNER JOIN dbo.Subscriber s ON swh.SubscriberId = s.SubscriberId 		
		WHERE swh.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
	)
    SELECT SubscriberWorkHistoryGuid WorkHistoryGuid
		, c.CompanyName
		, swh.Title JobTitle
		, swh.JobDescription
		, swh.StartDate BeginDate
		, swh.EndDate
		, CAST(ISNULL(swh.IsCurrent, 0) AS BIT) IsCurrent
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM dbo.SubscriberWorkHistory swh
	INNER JOIN dbo.Company c ON swh.CompanyId = c.CompanyId
	INNER JOIN dbo.Subscriber s ON swh.SubscriberId = s.SubscriberId 		
    WHERE swh.IsDeleted = 0
	AND s.SubscriberGuid = @SubscriberGuid
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''companyName'' THEN c.CompanyName END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''isCurrent'' THEN CAST(ISNULL(swh.IsCurrent, 0) AS BIT) END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''beginDate'' THEN swh.StartDate END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''endDate'' THEN swh.EndDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN swh.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN swh.ModifyDate END,	
    CASE WHEN @Order = ''descending'' AND @Sort = ''companyName'' THEN c.CompanyName END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''isCurrent'' THEN CAST(ISNULL(swh.IsCurrent, 0) AS BIT) END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''beginDate'' THEN swh.StartDate END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''endDate'' THEN swh.EndDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN swh.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN swh.ModifyDate END DESC
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_SubscriberWorkHistory]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Update_SubscriberWorkHistory]");
            migrationBuilder.Sql("DROP TYPE [dbo].[SubscriberWorkHistory]");
        }
    }
}
