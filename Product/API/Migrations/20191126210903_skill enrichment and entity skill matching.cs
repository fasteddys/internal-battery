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
</remarks>
<description>
Returns a list of jobs based on the skills that are associated with the course provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. A subscriber is optional, but if provided the results are further weighted according
to the distance of each job from the subscriber''s location.
</description>
<example>
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''EC7243F5-3117-447F-A945-92835500F364'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 5, @Offset = 5
EXEC [dbo].[System_Get_JobsByCourse] @CourseGuid = ''EC7243F5-3117-447F-A945-92835500F364'', @SubscriberGuid = NULL, @Limit = 50, @Offset = 0
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
			SELECT jp.JobPostingGuid, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) [WeightedSkillScore]
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
			SELECT jp.JobPostingGuid, jp.PostalId, jp.PostingDateUTC, c.CompanyName, c.LogoUrl, jp.Title, i.[Name] Industry, jc.[Name] JobCategory, jp.Country, jp.Province, jp.City, CAST(COUNT(aswhr.SkillId) AS DECIMAL) / CAST(AVG(aswhr.HighestSkillScore) AS DECIMAL) [WeightedSkillScore]
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
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, jwss.City, WeightedSkillScore, dbo.fn_GetGeoDistance(@subscriberLatitude, @subscriberLongitude, p.Latitude, p.Longitude) [DistanceInMeters]
			FROM jobsWithSkillScore jwss
			INNER JOIN Postal p WITH(NOLOCK) ON jwss.PostalId = p.PostalId)
		, jobsWithDistanceIndex AS (
			-- the distance index groups jobs with similar distance so that we can perform a more complex sort that includes weighted skill score
			SELECT JobPostingGuid, PostingDateUTC, CompanyName, LogoUrl, Title, Industry, JobCategory, Country, Province, City, WeightedSkillScore, DistanceInMeters, NTILE(10) OVER (ORDER BY DistanceInMeters ASC) [DistanceIndex]
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
2019.11.26 - Bill Koenig - Created
</remarks>
<description>
Finds the postal record which is the best match for a given job based on the fields that have been populated for the job. The prioritization for the match
is a postal code match first, a city and state match second, and a state match third. Modifications to deleted jobs have no effect, nor do inserts or updates
that do not include a geo field in the job record.
</description>
<example>
UPDATE dbo.JobPosting SET postalcode = 90210 WHERE JobPostingId = 10245
</example>
*/
CREATE TRIGGER [dbo].[TR_JobPosting_MatchPostal]
    ON [dbo].[JobPosting]
    AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(PostalCode) OR UPDATE(City) OR UPDATE (Province)
	BEGIN
		WITH geoData AS (
			SELECT p.PostalId, p.Code [PostalCode], ci.CityId, ci.[Name] City, s.StateId, s.Code StateCode, s.[Name] StateName, co.CountryId, co.Code2 [Country]
			FROM [State] s WITH(NOLOCK)
			INNER JOIN Country co WITH(NOLOCK) ON s.CountryId = co.CountryId
			INNER JOIN City ci WITH(NOLOCK) ON s.StateId = ci.StateId 
			INNER JOIN Postal p WITH(NOLOCK) ON ci.CityId = p.CityId)
		, jobData AS (
			SELECT j.JobPostingId, j.Province, j.City, j.PostalCode
			FROM JobPosting j WITH(NOLOCK)
			INNER JOIN Inserted i ON j.JobPostingId = i.JobPostingId
			WHERE i.IsDeleted = 0)
		, jobAndGeoData AS (
			SELECT j.JobPostingId, g.PostalId, 3 [Priority]
			FROM jobData j
			INNER JOIN geoData g ON j.Province = g.StateCode OR j.Province = g.StateName
			UNION
			SELECT j.JobPostingId, g.PostalId, 2
			FROM jobData j
			INNER JOIN geoData g ON j.City = g.City AND (j.Province = g.StateCode OR j.Province = g.StateName)
			UNION
			SELECT j.JobPostingId, g.PostalId, 1
			FROM jobData j
			INNER JOIN geoData g ON j.PostalCode = g.PostalCode)
		, prioritizedJobAndGeoData AS (
			SELECT JobPostingId, PostalId, [Priority], ROW_NUMBER() OVER (PARTITION BY JobPostingId ORDER BY [Priority] ASC) [RowNum]
			FROM jobAndGeoData)
		, bestMatchGeo AS (
			SELECT JobPostingId, PostalId 
			FROM prioritizedJobAndGeoData
			WHERE RowNum = 1)
		UPDATE 
			j 
		SET 
			PostalId = bmg.PostalId
			, ModifyDate = GETUTCDATE()
			, ModifyGuid = ''00000000-0000-0000-0000-000000000000''
		FROM 
			dbo.JobPosting j
			INNER JOIN bestMatchGeo bmg ON j.JobPostingId = bmg.JobPostingId
	END 
END
            ')");

            migrationBuilder.Sql("ALTER TABLE [dbo].[JobPosting] ENABLE TRIGGER [TR_JobPosting_MatchPostal]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [dbo].[RelatedJobSkillMatrix]");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_RelatedJobSkillMatrix]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_JobsByCourse]");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_GetGeoDistance]");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_RelatedJobSkills]");
            migrationBuilder.Sql(@"DROP TRIGGER [dbo].[TR_JobPosting_MatchPostal]");
        }
    }
}
