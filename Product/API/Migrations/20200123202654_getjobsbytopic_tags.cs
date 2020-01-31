using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class getjobsbytopic_tags : Migration
    {
       protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2020.1.23 - Jyoti Guin - Updated the joins for course so it uses tags
</remarks>
<description>
Returns jobs associated with a topic
</description>
 */
    ALTER PROCEDURE [dbo].[System_Get_JobsByTopic] (
        @TopicGuid UNIQUEIDENTIFIER,
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
                INNER JOIN TagCourse tc WITH(NOLOCK) ON c.courseId = tc.courseId
                INNER JOIN Tag t WITH(NOLOCK) ON t.tagId = tc.tagid
                INNER JOIN TagTopic tt WITH(NOLOCK) ON tt.tagId = t.tagid
        		INNER JOIN Topic p WITH(NOLOCK) ON p.TopicId = tt.TopicId
                INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
                WHERE p.TopicGuid = @TopicGuid
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
                INNER JOIN TagCourse tc WITH(NOLOCK) ON c.courseId = tc.courseId
                INNER JOIN Tag t WITH(NOLOCK) ON t.tagId = tc.tagid
                INNER JOIN TagTopic tt WITH(NOLOCK) ON tt.tagId = t.tagid
        		INNER JOIN Topic p WITH(NOLOCK) ON p.TopicId = tt.TopicId
                INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
                WHERE p.TopicGuid = @TopicGuid
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
