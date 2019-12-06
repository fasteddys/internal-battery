using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class skillenrichmentandentityskillmatching : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [dbo].[RelatedJobSkillMatrix](
    [SkillId] [int] NOT NULL,
    [PopularityScore] [decimal](10, 9) NOT NULL,
    [PopularityIndex] [int] NOT NULL,
    [RelatedSkillId] [int] NOT NULL,
    [MatchScore] [decimal](10, 9) NOT NULL,
    [MatchIndex] [int] NOT NULL
) ON[PRIMARY]
            ");

            migrationBuilder.Sql(@"

CREATE INDEX [IX_RelatedJobSkillMatrix_SkillId]
ON [RelatedJobSkillMatrix]
(
	SkillId ASC
)

CREATE INDEX [IX_RelatedJobSkillMatrix_RelatedSkillId]
ON [RelatedJobSkillMatrix]
(
	RelatedSkillId ASC
)

CREATE NONCLUSTERED INDEX [IX_RelatedJobSkillMatrix_SkillId_RelatedSkillId] ON [dbo].[RelatedJobSkillMatrix]
(
	[SkillId] ASC,
	[RelatedSkillId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
            ");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-11-20 - Bill Koenig - Created
</remarks>
<description>
Returns the distance in meters between two points using latitude and longitude.
</description>
<example>
SELECT [dbo].[fn_GetGeoDistance](39.183131, 76.7300285, 39.1950823, 76.7820398)
SELECT [dbo].[fn_GetGeoDistance](39.183131, 76.7300285, NULL, NULL)
</example>
*/
CREATE FUNCTION [dbo].[fn_GetGeoDistance](@lat1 FLOAT, @lng1 FLOAT, @lat2 FLOAT, @lng2 FLOAT)
RETURNS FLOAT
AS
BEGIN

	IF @lat1 IS NULL OR @lng1 IS NULL OR @lat2 IS NULL OR @lng2 IS NULL
	BEGIN
		RETURN NULL;
	END

	DECLARE @p1 geography = geography::Point(@lat1, @lng1, 4326);
	DECLARE @p2 geography = geography::Point(@lat2, @lng2, 4326);	
	
	RETURN @p1.STDistance(@p2);
END                
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-11-18 - Bill Koenig - Created
</remarks>
<description>
Returns a list of skills which appeared alongside the skill provided amongst all job postings.
Occurrences is the number of times the related skill appeared alongside the skill provided.
</description>
<example>
SELECT * FROM [dbo].[fn_RelatedJobSkills](21257) ORDER BY Occurrences DESC
</example>
*/
CREATE FUNCTION [dbo].[fn_RelatedJobSkills](@SkillId INT)
RETURNS TABLE
AS
RETURN  
	WITH jobsWithSkill AS (
		SELECT JobPostingId
		FROM JobPostingSkill
		WHERE skillid = @SkillId
	)
	SELECT jps.SkillId, COUNT(1) Occurrences
	FROM JobPostingSkill jps
	INNER JOIN jobsWithSkill jws ON jps.JobPostingId = jws.JobPostingId
	WHERE jps.SkillId <> @SkillId
	GROUP BY jps.SkillId
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-11-19 - Bill Koenig - Created
</remarks>
<description>
Returns all skills that appear in job postings (active or expired). Popularity score is the number of times the skill was associated with a job divided by the 
total number of jobs that had at least one skill. Popularity index is a decile representation of the the popularity score (lower is better). Related skills 
are those that appeared for the same job posting alongside a given skill. Match score is the number of times the related skill appeared alongside a given skill
divided by the number of times the target skill was associated with a job posting. Match index is a decile representation of the match score (lower is better).
</description>
<example>
SELECT * 
FROM [dbo].[v_RelatedJobSkillMatrix] rjsm
INNER JOIN [dbo].[Skill] s ON rjsm.SkillId = s.SkillId
WHERE s.SkillName = ''complex problem solving''
ORDER BY PopularityIndex ASC, PopularityScore DESC, MatchIndex ASC, MatchScore DESC
</example>
*/
CREATE VIEW [dbo].[v_RelatedJobSkillMatrix]
AS
	WITH jobSkills AS (
		SELECT s.SkillId
			, CAST(COUNT(distinct jps.JobPostingId) AS DECIMAL) / CAST((SELECT COUNT(DISTINCT JobPostingId) FROM JobPostingSkill) AS DECIMAL) [PopularityScore]
		FROM Skill s
		INNER JOIN JobPostingSkill jps on s.SkillId = jps.SkillId
		group by s.SkillId, s.SkillName
	), jobSkillsWithPopularityIndex AS (
		SELECT SkillId
			, PopularityScore
			, NTILE(10) OVER (ORDER BY popularityscore desc) [PopularityIndex]
		FROM jobSkills
	), jobSkillsWithPopularityIndexAndRelatedSkills AS (
		SELECT jswpi.SkillId
			, jswpi.PopularityScore
			, jswpi.PopularityIndex
			, rs.SkillId [RelatedSkillId]
			, CAST(rjs.Occurrences AS DECIMAL) / CAST((SELECT COUNT(1) FROM JobPostingSkill WHERE skillid = jswpi.SkillId) AS DECIMAL) MatchScore
		FROM jobSkillsWithPopularityIndex jswpi
		CROSS APPLY dbo.fn_RelatedJobSkills(jswpi.SkillId) rjs
		INNER JOIN Skill rs ON rjs.SkillId = rs.SkillId
	)
	SELECT jswpiars.SkillId
		, jswpiars.PopularityScore
		, jswpiars.PopularityIndex
		, jswpiars.RelatedSkillId
		, jswpiars.MatchScore
		, NTILE(10) OVER (PARTITION BY jswpiars.skillid ORDER BY matchscore desc) MatchIndex
	FROM jobSkillsWithPopularityIndexAndRelatedSkills jswpiars
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.11.22 - Bill Koenig - Created
2019.12.06 - Bill Koenig - Adjusted data types to fix mapping issues in EF
</remarks>
<description>
Returns a list of jobs based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. A subscriber is optional, but if provided the results are further weighted according
to the distance of each job from the subscriber''s location.
</description>
<example>
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''EC7243F5-3117-447F-A945-92835500F364'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 5, @Offset = 5
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''EC7243F5-3117-447F-A945-92835500F364'', @SubscriberGuid = NULL, @Limit = 100, @Offset = 0
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_JobsByCourse] (
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
			SELECT cs.SkillId, s.SkillName
			FROM Course c
			INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
			INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
			WHERE c.CourseGuid = @courseGuid
			AND cs.IsDeleted = 0)
		, courseSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
			FROM courseSkills cs WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
			INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
			UNION 
			SELECT cs.SkillId, cs.SkillName, ISNULL(rjsm.PopularityIndex, 10)
			FROM courseSkills cs
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
			FROM courseSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId, SkillName)
		, jobsWithRank AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingGuid, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
			LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
			LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingGuid, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name], jc.[Name], jp.Country, jp.Province, jp.City)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, NULL DistanceInMeters, NULL DistanceIndex
		FROM jobsWithRank jwr
		ORDER BY WeightedSkillScore DESC		
		OFFSET @Offset ROWS
		FETCH FIRST @Limit ROWS ONLY
	END
	ELSE
	BEGIN
		-- sort by distance first then by weighted skill score
		;WITH courseSkills AS (
			-- skills that are associated with the course
			SELECT cs.SkillId, s.SkillName
			FROM Course c
			INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
			INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
			WHERE c.CourseGuid = @courseGuid
			AND cs.IsDeleted = 0)
		, courseSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
			FROM courseSkills cs WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
			INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
			UNION 
			SELECT cs.SkillId, cs.SkillName, ISNULL(rjsm.PopularityIndex, 10)
			FROM courseSkills cs
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
			FROM courseSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId, SkillName)
		, jobsWithSkillScore AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingGuid, jp.PostalId, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
			LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
			LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingGuid, jp.PostalId, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name], jc.[Name], jp.Country, jp.Province, jp.City)
		, jobsWithDistance AS (
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, jwss.City, WeightedSkillScore, CAST(dbo.fn_GetGeoDistance(@subscriberLatitude, @subscriberLongitude, p.Latitude, p.Longitude) AS DECIMAL(10,2)) [DistanceInMeters]
			FROM jobsWithSkillScore jwss
			INNER JOIN Postal p WITH(NOLOCK) ON jwss.PostalId = p.PostalId)
		, jobsWithDistanceIndex AS (
			-- the distance index groups jobs with similar distance so that we can perform a more complex sort that includes weighted skill score
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, CAST(NTILE(10) OVER (ORDER BY DistanceInMeters ASC) AS INT) [DistanceIndex]
			FROM jobsWithDistance
			WHERE DistanceInMeters IS NOT NULL)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, DistanceIndex
		FROM jobsWithDistanceIndex
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
</remarks>
<description>
Returns a list of jobs based on the skills that are associated with the subscriber provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. The results are further weighted according to the distance of each job from the 
subscriber''s location.
</description>
<example>
EXEC [dbo].[System_Get_JobsBySubscriber] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 15, @Offset = 0
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_JobsBySubscriber] (
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
			SELECT ss.SkillId, k.SkillName
			FROM Subscriber s
			INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
			INNER JOIN Skill k WITH(NOLOCK) ON ss.SkillId = k.SkillId
			WHERE s.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0)
		, subscriberSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
			FROM subscriberSkills ss WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
			INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
			UNION 
			SELECT ss.SkillId, ss.SkillName, ISNULL(rjsm.PopularityIndex, 10)
			FROM subscriberSkills ss
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
			FROM subscriberSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId, SkillName)
		, jobsWithRank AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingGuid, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
			LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
			LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingGuid, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name], jc.[Name], jp.Country, jp.Province, jp.City)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, NULL DistanceInMeters, NULL DistanceIndex
		FROM jobsWithRank jwr
		ORDER BY WeightedSkillScore DESC		
		OFFSET @Offset ROWS
		FETCH FIRST @Limit ROWS ONLY
	END
	ELSE
	BEGIN
		-- sort by distance first then by weighted skill score
		;WITH subscriberSkills AS (
			-- skills that are associated with the subscriber
			SELECT ss.SkillId, k.SkillName
			FROM Subscriber s
			INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
			INNER JOIN Skill k WITH(NOLOCK) ON ss.SkillId = k.SkillId
			WHERE s.SubscriberGuid = @SubscriberGuid
			AND ss.IsDeleted = 0)
		, subscriberSkillsAndRelatedSkillsWithRank AS (
			-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
			SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
			FROM subscriberSkills ss WITH(NOLOCK)
			INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
			INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
			UNION 
			SELECT ss.SkillId, ss.SkillName, ISNULL(rjsm.PopularityIndex, 10)
			FROM subscriberSkills ss
			LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
		, allSkillsWithHighestRank AS (
			-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
			SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
			FROM subscriberSkillsAndRelatedSkillsWithRank 
			GROUP BY SkillId, SkillName)
		, jobsWithSkillScore AS (
			-- associates these finalized list of skills with jobs and adds a weighted rank based on the number of skills matched to the job and the score associated with each skill
			SELECT jp.JobPostingGuid, jp.PostalId, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
			FROM allSkillsWithHighestRank aswhr
			INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON aswhr.SkillId = jps.SkillId
			INNER JOIN JobPosting jp WITH(NOLOCK) ON jps.JobPostingId = jp.JobPostingId
			INNER JOIN Company c WITH(NOLOCK) ON c.CompanyId = jp.CompanyId
			LEFT JOIN Industry i WITH(NOLOCK) ON jp.IndustryId = i.IndustryId
			LEFT JOIN JobCategory jc WITH(NOLOCK) ON jp.JobCategoryId = jc.JobCategoryId
			WHERE jp.IsDeleted = 0 
			AND jps.IsDeleted = 0
			GROUP BY jp.JobPostingGuid, jp.PostalId, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name], jc.[Name], jp.Country, jp.Province, jp.City)
		, jobsWithDistance AS (
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, jwss.City, WeightedSkillScore, CAST(dbo.fn_GetGeoDistance(@subscriberLatitude, @subscriberLongitude, p.Latitude, p.Longitude) AS DECIMAL(10,2)) [DistanceInMeters]
			FROM jobsWithSkillScore jwss
			INNER JOIN Postal p WITH(NOLOCK) ON jwss.PostalId = p.PostalId)
		, jobsWithDistanceIndex AS (
			-- the distance index groups jobs with similar distance so that we can perform a more complex sort that includes weighted skill score
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, CAST(NTILE(10) OVER (ORDER BY DistanceInMeters ASC) AS INT) [DistanceIndex]
			FROM jobsWithDistance
			WHERE DistanceInMeters IS NOT NULL)
		SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, DistanceIndex
		FROM jobsWithDistanceIndex
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
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesByCourse] @CourseGuid = ''EC7243F5-3117-447F-A945-92835500F364'', @Limit = 15, @Offset = 0
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_CoursesByCourse] (
	@CourseGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	
	;WITH courseSkills AS (
		-- skills that are associated with the subscriber
		SELECT cs.SkillId, s.SkillName
		FROM Course c
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
		INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
		WHERE c.CourseGuid = @CourseGuid
		AND cs.IsDeleted = 0)
	, courseSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
		FROM courseSkills cs WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId
		INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
		UNION 
		SELECT cs.SkillId, cs.SkillName, ISNULL(rjsm.PopularityIndex, 10)
		FROM courseSkills cs
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON cs.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
		FROM courseSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId, SkillName)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT c.CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		INNER JOIN Course c WITH(NOLOCK) ON cs.CourseId = c.CourseId
		INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
		WHERE c.IsDeleted = 0 
		AND cs.IsDeleted = 0
		AND c.CourseGuid <> @CourseGuid
		GROUP BY c.CourseGuid, c.[Name], c.[Description], c.TabletImage, v.[Name], v.LogoUrl)
	SELECT CourseGuid, CourseName, CourseDescription, CourseLogoUrl, VendorName, VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
	
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.06 - Bill Koenig - Created
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesByJob] @JobPostingGuid = ''46B539E4-E54B-4E68-ABB1-8AC674F8FD87'', @Limit = 5, @Offset = 0
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_CoursesByJob] (
	@JobPostingGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT
)
AS
BEGIN 
	SET NOCOUNT ON;
	
	;WITH jobSkills AS (
		-- skills that are associated with the subscriber
		SELECT s.SkillId, s.SkillName
		FROM JobPosting j
		INNER JOIN JobPostingSkill jps WITH(NOLOCK) ON jps.JobPostingId = j.JobPostingId
		INNER JOIN Skill s WITH(NOLOCK) ON jps.SkillId = s.SkillId
		WHERE j.JobPostingGuid = @JobPostingGuid
		AND jps.IsDeleted = 0)
	, jobSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
		FROM jobSkills js WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId
		INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
		UNION 
		SELECT js.SkillId, js.SkillName, ISNULL(rjsm.PopularityIndex, 10)
		FROM jobSkills js
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON js.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
		FROM jobSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId, SkillName)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT c.CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		INNER JOIN Course c WITH(NOLOCK) ON cs.CourseId = c.CourseId
		INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
		WHERE c.IsDeleted = 0 
		AND cs.IsDeleted = 0
		GROUP BY c.CourseGuid, c.[Name], c.[Description], c.TabletImage, v.[Name], v.LogoUrl)
	SELECT CourseGuid, CourseName, CourseDescription, CourseLogoUrl, VendorName, VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
	
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.06 - Bill Koenig - Created
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the subscriber provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesBySubscriber] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 15, @Offset = 0
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_CoursesBySubscriber] (
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
		SELECT ss.SkillId, k.SkillName
		FROM Subscriber s
		INNER JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
		INNER JOIN Skill k WITH(NOLOCK) ON ss.SkillId = k.SkillId
		WHERE s.SubscriberGuid = @SubscriberGuid
		AND ss.IsDeleted = 0)
	, subscriberSkillsAndRelatedSkillsWithRank AS (
		-- includes skills that were associated with the above skills in job results; uses their match index to rank them with the directly matched skills
		SELECT rs.SkillId, rs.SkillName, rjsm.MatchIndex SkillScore
		FROM subscriberSkills ss WITH(NOLOCK)
		INNER JOIN [RelatedJobSkillMatrix] rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId
		INNER JOIN Skill rs WITH(NOLOCK) ON rjsm.RelatedSkillId = rs.SkillId
		UNION 
		SELECT ss.SkillId, ss.SkillName, ISNULL(rjsm.PopularityIndex, 10)
		FROM subscriberSkills ss
		LEFT JOIN RelatedJobSkillMatrix rjsm WITH(NOLOCK) ON ss.SkillId = rjsm.SkillId)
	, allSkillsWithHighestRank AS (
		-- chooses the highest index value for each skill (regardless whether it comes from the popularity index or match index)
		SELECT SkillId, SkillName, MIN(SkillScore) HighestSkillScore
		FROM subscriberSkillsAndRelatedSkillsWithRank 
		GROUP BY SkillId, SkillName)
	, coursesWithRank AS (
		-- associates these finalized list of skills with courses and adds a weighted rank based on the number of skills matched to the course and the score associated with each skill
		SELECT c.CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage CourseLogoUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, CAST(CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) AS DECIMAL(10,5)) [WeightedSkillScore]
		FROM allSkillsWithHighestRank aswhr
		INNER JOIN CourseSkill cs WITH(NOLOCK) ON aswhr.SkillId = cs.SkillId
		INNER JOIN Course c WITH(NOLOCK) ON cs.CourseId = c.CourseId
		INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
		WHERE c.IsDeleted = 0 
		AND cs.IsDeleted = 0
		GROUP BY c.CourseGuid, c.[Name], c.[Description], c.TabletImage, v.[Name], v.LogoUrl)
	SELECT CourseGuid, CourseName, CourseDescription, CourseLogoUrl, VendorName, VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	ORDER BY WeightedSkillScore DESC		
	OFFSET @Offset ROWS
	FETCH FIRST @Limit ROWS ONLY
END
            ')");

            migrationBuilder.Sql("ALTER TABLE [dbo].[JobPosting] ENABLE TRIGGER [TR_JobPosting_MatchPostal]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [dbo].[RelatedJobSkillMatrix]");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_RelatedJobSkillMatrix]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_JobsByCourse]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_JobsBySubscriber]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_CoursesByCourse]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_CoursesBySubscriber]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_CoursesByJob]");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_GetGeoDistance]");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_RelatedJobSkills]");
            migrationBuilder.Sql("DROP TRIGGER [dbo].[TR_JobPosting_MatchPostal]");
        }
    }
}
