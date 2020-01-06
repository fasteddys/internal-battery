using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class performanceimprovementsforrelatedentityqueries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.11.22 - Bill Koenig - Created
2019.12.06 - Bill Koenig - Adjusted data types to fix mapping issues in EF
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
</remarks>
<description>
Returns a list of jobs based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. A subscriber is optional, but if provided the results are further weighted according
to the distance of each job from the subscriber''s location.
</description>
<example>
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''BFE27EF8-A577-4D5B-A535-9FD4E614F449'', @SubscriberGuid = ''0A464E0C-255A-4F54-81B1-CE897E859C9E'', @Limit = 5, @Offset = 5
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''BFE27EF8-A577-4D5B-A535-9FD4E614F449'', @SubscriberGuid = NULL, @Limit = 100, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_JobsByCourse] (
    @CourseGuid UNIQUEIDENTIFIER,
    @SubscriberGuid UNIQUEIDENTIFIER = NULL,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
    SET NOCOUNT ON;
    DECLARE @subscriberLatitude FLOAT, @subscriberLongitude FLOAT;

    IF(@SubscriberGuid IS NOT NULL)
    BEGIN
    	;WITH geoData AS (
    		SELECT s.StateId, c.CountryId, ci.[Name] City, p.Code PostalCode, p.Latitude, p.Longitude
    		FROM [State] s WITH(NOLOCK)
    		INNER JOIN Country c WITH(NOLOCK) ON s.CountryId = c.CountryId
    		INNER JOIN City ci WITH(NOLOCK) ON s.StateId = ci.StateId 
    		INNER JOIN Postal p WITH(NOLOCK) ON ci.CityId = p.CityId)
    	, subData AS (
    		SELECT StateId, City, PostalCode
    		FROM Subscriber WITH(NOLOCK)
    		WHERE SubscriberGuid = @SubscriberGuid)
    	, prioritizedGeo AS (
    		SELECT TOP 1 g.Latitude, g.Longitude, 3 [Priority]
    		FROM subData s
    		INNER JOIN geoData g ON s.StateId = g.StateId
    		UNION
    		SELECT TOP 1 g.Latitude, g.Longitude, 2
    		FROM subData s
    		INNER JOIN geoData g ON s.City = g.City AND s.StateId = g.StateId
    		UNION
    		SELECT TOP 1 g.Latitude, g.Longitude, 1
    		FROM subData s
    		INNER JOIN geoData g ON s.PostalCode = g.PostalCode)
    	SELECT TOP 1 @subscriberLatitude = LATITUDE, @subscriberLongitude = LONGITUDE
    	FROM prioritizedGeo
    	ORDER BY [Priority] ASC
    END

    IF(@subscriberLatitude IS NULL OR @subscriberLongitude IS NULL)
    BEGIN
    	-- do not evaluate geo; return results based on weighted skill score only
    	;WITH courseSkills AS (
    		-- skills that are associated with the course
    		SELECT cs.SkillId
    		FROM Course c
    		INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
    		WHERE c.CourseGuid = @courseGuid
    		AND cs.IsDeleted = 0)
    	, courseSkillsAndRelatedSkillsWithRank AS (
    		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
    		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
    		FROM courseSkills cs WITH(NOLOCK)
    		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
    		UNION 
    		SELECT cs.SkillId, ISNULL(rjsm.PopularityIndex, 10)
    		FROM courseSkills cs
    		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
    	, allSkillsWithHighestRank AS (
    		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
    		SELECT SkillId, MIN(SkillScore) HighestSkillScore
    		FROM courseSkillsAndRelatedSkillsWithRank 
    		GROUP BY SkillId)
    	, jobsWithRank AS (
    		-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
    		SELECT jp.JobPostingId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
    		FROM allSkillsWithHighestRank aswhr
    		INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
    		INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
    		WHERE jp.IsDeleted = 0 
    		AND jps.IsDeleted = 0
    		GROUP BY jp.JobPostingId)
    	SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, i.[Name] Industry, jc.[Name] JobCategory, Country, Province, City, WeightedSkillScore, NULL DistanceInMeters, NULL DistanceIndex
    	FROM jobsWithRank jwr		
		INNER JOIN JobPosting jp WITH(NOLOCK) ON jwr.JobPostingId = jp.JobPostingId
		INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
		LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
		LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
    	ORDER BY WeightedSkillScore DESC		
    	OFFSET @Offset ROWS
    	FETCH FIRST @Limit ROWS ONLY
    END
    ELSE
    BEGIN
    	-- sort by distance first then by weighted skill score
    	;WITH courseSkills AS (
    		-- skills that are associated with the course
    		SELECT cs.SkillId
    		FROM Course c
    		INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
    		WHERE c.CourseGuid = @courseGuid
    		AND cs.IsDeleted = 0)
    	, courseSkillsAndRelatedSkillsWithRank AS (
    		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
    		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
    		FROM courseSkills cs WITH(NOLOCK)
    		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
    		UNION 
    		SELECT cs.SkillId, ISNULL(rjsm.PopularityIndex, 10)
    		FROM courseSkills cs
    		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
    	, allSkillsWithHighestRank AS (
    		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
    		SELECT SkillId, MIN(SkillScore) HighestSkillScore
    		FROM courseSkillsAndRelatedSkillsWithRank 
    		GROUP BY SkillId)
    	, jobsWithSkillScore AS (
    		-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
    		SELECT jp.JobPostingId, jp.PostalId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
    		FROM allSkillsWithHighestRank aswhr
    		INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
    		INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
    		WHERE jp.IsDeleted = 0 
    		AND jps.IsDeleted = 0
    		GROUP BY jp.JobPostingId, jp.PostalId)
    	, jobsWithDistance AS (
    		SELECT JobPostingId, WeightedSkillScore, CAST(dbo.fn_GetGeoDistance(@subscriberLatitude, @subscriberLongitude, p.Latitude, p.Longitude) AS DECIMAL(10,2)) [DistanceInMeters]
    		FROM jobsWithSkillScore jwss
    		INNER JOIN Postal p WITH(NOLOCK) ON jwss.PostalId = p.PostalId)
    	, jobsWithDistanceIndex AS (
    		-- the distance index groups jobs with similar distance so that we can perform a more complex sort that includes weighted skill score
    		SELECT JobPostingId, WeightedSkillScore, DistanceInMeters, CAST(NTILE(10) OVER (ORDER BY DistanceInMeters ASC) AS INT) [DistanceIndex]
    		FROM jobsWithDistance
    		WHERE DistanceInMeters IS NOT NULL)
    	SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, i.[Name] Industry, jc.[Name] JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, DistanceIndex
    	FROM jobsWithDistanceIndex jwdi
		INNER JOIN JobPosting jp WITH(NOLOCK) ON jwdi.JobPostingId = jp.JobPostingId
		INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
		LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
		LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
    	ORDER BY DistanceIndex ASC, WeightedSkillScore DESC
    	OFFSET @Offset ROWS
    	FETCH FIRST @Limit ROWS ONLY
    END
END
            ')");

            migrationBuilder.Sql(@"EXEC('

/*
<remarks>
2019.12.05 - Bill Koenig - Created
2019.12.06 - Bill Koenig - Adjusted data types to fix mapping issues in EF
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
</remarks>
<description>
Returns a list of jobs based on the skills that are associated with the subscriber provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. The results are further weighted according to the distance of each job from the 
subscriber''s location.
</description>
<example>
EXEC [dbo].[System_Get_JobsBySubscriber] @SubscriberGuid = ''71A7156E-173F-4054-83ED-AD6127BAFE87'', @Limit = 15, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_JobsBySubscriber] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	DECLARE @subscriberLatitude FLOAT, @subscriberLongitude FLOAT;

	;WITH geoData AS (
		SELECT s.StateId, c.CountryId, ci.[Name] City, p.Code PostalCode, p.Latitude, p.Longitude
		FROM [State] s WITH(NOLOCK)
		INNER JOIN Country c WITH(NOLOCK) ON s.CountryId = c.CountryId
		INNER JOIN City ci WITH(NOLOCK) ON s.StateId = ci.StateId 
		INNER JOIN Postal p WITH(NOLOCK) ON ci.CityId = p.CityId)
	, subData AS (
		SELECT StateId, City, PostalCode
		FROM Subscriber WITH(NOLOCK)
		WHERE SubscriberGuid = @SubscriberGuid)
	, prioritizedGeo AS (
		SELECT TOP 1 g.Latitude, g.Longitude, 3 [Priority]
		FROM subData s
		INNER JOIN geoData g ON s.StateId = g.StateId
		UNION
		SELECT TOP 1 g.Latitude, g.Longitude, 2
		FROM subData s
		INNER JOIN geoData g ON s.City = g.City AND s.StateId = g.StateId
		UNION
		SELECT TOP 1 g.Latitude, g.Longitude, 1
		FROM subData s
		INNER JOIN geoData g ON s.PostalCode = g.PostalCode)
	SELECT TOP 1 @subscriberLatitude = LATITUDE, @subscriberLongitude = LONGITUDE
	FROM prioritizedGeo
	ORDER BY [Priority] ASC

	IF(@subscriberLatitude IS NULL OR @subscriberLongitude IS NULL)
	BEGIN
		-- do not evaluate geo; return results based on weighted skill score only
		;WITH subscriberSkills AS (
			-- skills that are associated with the subscriber
			SELECT ss.SkillId
			FROM Subscriber s
			INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
			WHERE s.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0)
		, subscriberSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
			FROM subscriberSkills ss WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
			UNION 
			SELECT ss.SkillId, ISNULL(rjsm.PopularityIndex, 10)
			FROM subscriberSkills ss
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, MIN(SkillScore) HighestSkillScore
			FROM subscriberSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId)
		, jobsWithRank AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingId)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, i.[Name] Industry, jc.[Name] JobCategory, Country, Province, City, WeightedSkillScore, NULL DistanceInMeters, NULL DistanceIndex
		FROM jobsWithRank jwr
		INNER JOIN JobPosting jp WITH(NOLOCK) ON jwr.JobPostingId = jp.JobPostingId
		INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
		LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
		LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
		ORDER BY WeightedSkillScore DESC		
		OFFSET @Offset ROWS
		FETCH FIRST @Limit ROWS ONLY
	END
	ELSE
	BEGIN
		-- sort by distance first then by weighted skill score
		;WITH subscriberSkills AS (
			-- skills that are associated with the subscriber
			SELECT ss.SkillId
			FROM Subscriber s
			INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
			WHERE s.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0)
		, subscriberSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
			FROM subscriberSkills ss WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
			UNION 
			SELECT ss.SkillId, ISNULL(rjsm.PopularityIndex, 10)
			FROM subscriberSkills ss
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, MIN(SkillScore) HighestSkillScore
			FROM subscriberSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId)
		, jobsWithSkillScore AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingId, jp.PostalId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingId, jp.PostalId)
		, jobsWithDistance AS (
			SELECT JobPostingId, WeightedSkillScore, CAST(dbo.fn_GetGeoDistance(@subscriberLatitude, @subscriberLongitude, p.Latitude, p.Longitude) AS DECIMAL(10,2)) [DistanceInMeters]
			FROM jobsWithSkillScore jwss
			INNER JOIN Postal p WITH(NOLOCK) ON jwss.PostalId = p.PostalId)
		, jobsWithDistanceIndex AS (
			-- the distance index groups jobs with similar distance so that we can perform a more complex sort that includes weighted skill score
			SELECT JobPostingId, WeightedSkillScore, DistanceInMeters, CAST(NTILE(10) OVER (ORDER BY DistanceInMeters ASC) AS INT) [DistanceIndex]
			FROM jobsWithDistance
			WHERE DistanceInMeters IS NOT NULL)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, i.[Name] Industry, jc.[Name] JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, DistanceIndex
		FROM jobsWithDistanceIndex jwdi
		INNER JOIN JobPosting jp WITH(NOLOCK) ON jwdi.JobPostingId = jp.JobPostingId
		INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
		LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
		LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
		ORDER BY DistanceIndex ASC, WeightedSkillScore DESC
		OFFSET @Offset ROWS
		FETCH FIRST @Limit ROWS ONLY
	END
END
            ')");

            migrationBuilder.Sql(@"EXEC('			
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesByJob] @JobPostingGuid = ''52CEFB9E-0AD8-414E-8F48-A6709DBE2B0E'', @Limit = 5, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByJob] (
	@JobPostingGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	
	;WITH jobSkills AS (
		-- skills that are associated with the subscriber
		SELECT jps.SkillId
		FROM JobPosting j
		INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON jps.JobPostingId = j.JobPostingId
		WHERE j.JobPostingGuid = @JobPostingGuid
		AND jps.IsDeleted = 0)
	, jobSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
		FROM jobSkills js WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId
		UNION 
		SELECT js.SkillId, ISNULL(rjsm.PopularityIndex, 10)
		FROM jobSkills js
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, MIN(SkillScore) HighestSkillScore
		FROM jobSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT cs.CourseId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		WHERE cs.IsDeleted = 0
		GROUP BY cs.CourseId)
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	INNER JOIN Course c WITH(NOLOCK) ON cwr.CourseId = c.CourseId
	INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
	WHERE c.IsDeleted = 0
	AND v.IsDeleted = 0
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the subscriber provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesBySubscriber] @SubscriberGuid = ''71A7156E-173F-4054-83ED-AD6127BAFE87'', @Limit = 15, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesBySubscriber] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;

	-- do not evaluate geo; return results based on weighted skill score only
	;WITH subscriberSkills AS (
		-- skills that are associated with the subscriber
		SELECT ss.SkillId
		FROM Subscriber s
		INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
		WHERE s.SubscriberGuid = @SubscriberGuid
		AND ss.IsDeleted = 0)
	, subscriberSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
		FROM subscriberSkills ss WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
		UNION 
		SELECT ss.SkillId, ISNULL(rjsm.PopularityIndex, 10)
		FROM subscriberSkills ss
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, MIN(SkillScore) HighestSkillScore
		FROM subscriberSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT cs.CourseId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		WHERE cs.IsDeleted = 0
		GROUP BY cs.CourseId)
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	INNER JOIN Course c WITH(NOLOCK) ON cwr.CourseId = c.CourseId
	INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
	WHERE c.IsDeleted = 0
	AND v.IsDeleted = 0
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesByCourse] @CourseGuid = ''52D7719A-E77F-4E1F-AD38-F42B7FE327A0'', @Limit = 100, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByCourse] (
	@CourseGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	
	;WITH courseSkills AS (
		-- skills that are associated with the subscriber
		SELECT cs.SkillId
		FROM Course c
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
		WHERE c.CourseGuid = @CourseGuid
		AND cs.IsDeleted = 0)
	, courseSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rjsm.RelatedSkillId SkillId, rjsm.MatchIndex SkillScore
		FROM courseSkills cs WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
		UNION 
		SELECT cs.SkillId, ISNULL(rjsm.PopularityIndex, 10)
		FROM courseSkills cs
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, MIN(SkillScore) HighestSkillScore
		FROM courseSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT cs.CourseId, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		WHERE cs.IsDeleted = 0
		GROUP BY cs.CourseId)
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	INNER JOIN Course c WITH(NOLOCK) ON cwr.CourseId = c.CourseId
	INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
	WHERE c.IsDeleted = 0
	AND c.CourseGuid <> @CourseGuid
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
	
END
            ')");

            migrationBuilder.Sql(@"
DROP INDEX [IX_RelatedJobSkillMatrix_SkillId_RelatedSkillId] ON [dbo].[RelatedJobSkillMatrix]
GO

DROP INDEX [IX_RelatedJobSkillMatrix_SkillId] ON [dbo].[RelatedJobSkillMatrix]
GO

DROP INDEX [IX_RelatedJobSkillMatrix_RelatedSkillId] ON [dbo].[RelatedJobSkillMatrix]
GO

CREATE CLUSTERED INDEX [CIX_RelatedJobSkillMatrix_SkillId] ON [dbo].[RelatedJobSkillMatrix]
(
	[SkillId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_RelatedJobSkillMatrix_SkillIdWithPopularityIndex] ON [dbo].[RelatedJobSkillMatrix]
(
	[SkillId] ASC
)
INCLUDE
(
	[PopularityIndex]
)
GO

CREATE NONCLUSTERED INDEX [IX_RelatedJobSkillMatrix_RelatedSkillIdWithMatchIndex] ON [dbo].[RelatedJobSkillMatrix]
(
	[RelatedSkillId] ASC
)
INCLUDE
(
	[MatchIndex]
)
GO

CREATE NONCLUSTERED INDEX [IX_JobPosting_IsDeletedWithExtraFields] ON [dbo].[JobPosting]
(
	[IsDeleted] ASC
)
INCLUDE
(
	[JobPostingGuid],
	[PostingDateUTC],
	[CompanyId],
	[Title],
	[IndustryId],
	[JobCategoryId],
	[Country],
	[Province],
	[City],
	[PostalId]
)
GO

CREATE NONCLUSTERED INDEX [IX_CourseSkill_IsDeletedWithExtraFields] ON [dbo].[CourseSkill]
(
	[IsDeleted] ASC
)
INCLUDE
(
	[CourseId],
	[SkillId]
)
GO

CREATE NONCLUSTERED INDEX [IX_Company_IsDeletedWithExtraFields] ON [dbo].[Company]
(
	[IsDeleted] ASC
)
INCLUDE
(
	[CompanyId],
	[CompanyName]
)
GO

CREATE NONCLUSTERED INDEX [IX_Course_IsDeletedWithExtraFields] ON [dbo].[Course]
(
	[CourseGuid] ASC
)
INCLUDE
(
	[CourseId]
)
GO

CREATE NONCLUSTERED INDEX [IX_Postal_Code]
ON [dbo].[Postal] ([Code])
GO
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
