using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Course_DesktopImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('  
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
</remarks>
<description>
Retrieves course by course identifier
</description>
<example>
EXEC [dbo].[System_Get_Course] @CourseGuid = ''1BB354E8-6188-4A0B-BF39-DDAE615D9CC1''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_Course] (
    @CourseGuid UNIQUEIDENTIFIER
)
AS
BEGIN 
    SELECT TOP 1 c.Name AS Title
        ,NULL AS Duration
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
    	,c.Code
    	,cl.Name as Level
    	,c.CreateDate
    	,c.ModifyDate
    	,c.IsDeleted
    	,c.TabletImage
    	,c.DesktopImage
    	,c.MobileImage
        ,c.ThumbnailImage
    	,t.Name as Topic
    	,(
    			SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
    			FROM CourseSkill cs
    			INNER JOIN Skill s ON s.SkillId = cs.SkillId

    			WHERE 
    				cs.CourseId = c.CourseId AND
    				cs.IsDeleted = 0 AND 
    				s.IsDeleted = 0
    	) CourseSkills
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.CourseGuid = @CourseGuid
END   
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
</remarks>
<description>
Retrieves courses with filter options
</description>
<example>
EXEC [dbo].[System_Get_Courses] @Limit = 10, @Offset = 0, @Order = ''title'', @Sort = ''ascending''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_Courses] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    SELECT c.Name AS Title
        ,NULL AS Duration
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
    	,c.Code
    	,cl.Name as Level
    	,c.CreateDate
    	,c.ModifyDate
    	,c.IsDeleted
    	,c.TabletImage
    	,c.DesktopImage
    	,c.MobileImage
        ,c.ThumbnailImage
    	,t.Name as Topic
    	,(
    			SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
    			FROM CourseSkill cs
    			INNER JOIN Skill s ON s.SkillId = cs.SkillId

    			WHERE 
    				cs.CourseId = c.CourseId AND
    				cs.IsDeleted = 0 AND 
    				s.IsDeleted = 0
    	) CourseSkills
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
    ORDER BY 
    CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN c.Name END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''vendorName'' THEN v.Name END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN c.ModifyDate END,
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN c.Name  END desc,
    CASE WHEN @Sort = ''descending'' AND @Order = ''vendorName'' THEN v.Name END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN c.CreateDate END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN c.ModifyDate END desc 
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
2019.12.19 - Bill Koenig - Modifying image properties
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
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.18 - Bill Koenig - Created
2019.12.19 - Bill Koenig - Modifying image properties
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the courses provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
DECLARE @CourseGuids AS [dbo].[GuidList]
INSERT INTO @CourseGuids VALUES (''8E26FACE-6B65-46A5-909C-0D8AF14567AF'') 
INSERT INTO @CourseGuids VALUES (''C01AD389-1824-481E-BB7D-1BEC8F80D085'') 
INSERT INTO @CourseGuids VALUES (''F0221F28-9104-45EC-B453-297AFE9F4899'') 
EXEC [dbo].[System_Get_CoursesByCourses] @CourseGuids = @CourseGuids, @Limit = 100, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByCourses] (
	@CourseGuids dbo.GuidList READONLY,
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
		INNER JOIN @CourseGuids cg ON c.CourseGuid = cg.Guid
		WHERE cs.IsDeleted = 0)
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
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
	FROM coursesWithRank cwr
	INNER JOIN Course c WITH(NOLOCK) ON cwr.CourseId = c.CourseId
	INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
	LEFT JOIN @CourseGuids cg ON c.CourseGuid = cg.Guid
	WHERE c.IsDeleted = 0
	AND cg.[Guid] IS NULL
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
2019.12.19 - Bill Koenig - Modifying image properties
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
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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
2019.12.18 - Bill Koenig - Created
2019.12.19 - Bill Koenig - Modifying image properties
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the jobs provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
DECLARE @JobPostingGuids AS [dbo].[GuidList]
INSERT INTO @JobPostingGuids VALUES (''39197C5F-5A6B-4107-95C6-0BBA161EE639'') 
INSERT INTO @JobPostingGuids VALUES (''84AE2339-EE3F-414B-BAAD-E0F6C573BD6E'') 
INSERT INTO @JobPostingGuids VALUES (''D4A943FB-2E27-4D50-AEB0-D3A7F06493CB'') 
INSERT INTO @JobPostingGuids VALUES (''3D5D8DB9-D65E-4E41-A589-2F1F0D5D7396'')
EXEC [dbo].[System_Get_CoursesByJobs] @JobPostingGuids = @JobPostingGuids, @Limit = 100, @Offset = 0
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByJobs] (
	@JobPostingGuids dbo.GuidList READONLY,
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
		INNER JOIN @JobPostingGuids jpg ON j.JobPostingGuid = jpg.Guid
		WHERE jps.IsDeleted = 0)
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
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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
2019.12.19 - Bill Koenig - Modifying image properties
</remarks>
<description>
Returns a list of courses based on the skills that are associated with the subscriber provided. The skill list is enriched by including skills that appear in the related job skill matrix.
Results are weighted according to the skill score. The operation supports a limit and offset. 
</description>
<example>
EXEC [dbo].[System_Get_CoursesBySubscriber] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9'', @Limit = 15, @Offset = 0
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
	SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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
2019.10.24 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level, cleaned up formatting
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
</remarks>
<description>
Select random courses 
</description>
<example>
EXEC [dbo].[System_Get_CoursesRandom] @MaxResults = 5
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesRandom] (
	@MaxResults INT
)
AS
BEGIN 
	SELECT TOP (@MaxResults)
	c.Name as Title ,
		null as Duration,
		c.Description, 
		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
		v.Name as VendorName,
		c.CourseGuid,
		v.LogoUrl as VendorLogoUrl ,
		v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid
		,c.Code
		,cl.Name as Level
		,c.CreateDate
		,c.ModifyDate
		,c.IsDeleted
		,c.TabletImage
		,c.DesktopImage
		,c.MobileImage
		,c.ThumbnailImage
		,t.Name as Topic
		,(
				SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
				FROM CourseSkill cs
				INNER JOIN Skill s ON s.SkillId = cs.SkillId

				WHERE 
					cs.CourseId = c.CourseId AND
					cs.IsDeleted = 0 AND 
					s.IsDeleted = 0
	   ) CourseSkills
    FROM  Course c  
    	LEFT JOIN Vendor v ON v.VendorId = c.VendorId
        LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
		LEFT JOIN Topic t on c.TopicId = t.TopicId
    ORDER by newid()          
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.17 - Jim Brazil - Created
2019.12.19 - Bill Koenig - Modifying image properties, adding example
</remarks>
<description>
Retrieves coursevariants by course identifier
</description>
<example>
EXEC [dbo].[System_Get_CourseVariants] @CourseGuid = ''F2B8C6F4-E34C-4205-A014-7AE181807C90''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CourseVariants] (
    @CourseGuid UNIQUEIDENTIFIER
)
AS
BEGIN 
    SELECT c.Name AS Title
        ,NULL AS Duration
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid,
		cv.CourseVariantGuid,
		cv.Price,
		cvt.Name as CourseVariantType,
		c.Code,
		cl.Name as Level,
		c.CreateDate,
		c.ModifyDate,
		c.IsDeleted,
		c.TabletImage,
		c.DesktopImage,
		c.MobileImage,
		c.ThumbnailImage,
		t.Name as Topic,

		
			(
				SELECT STRING_AGG(s.SkillName, ''; '') AS Skills
				FROM CourseSkill cs
				INNER JOIN Skill s ON s.SkillId = cs.SkillId

				WHERE 
					cs.CourseId = c.CourseId AND
					cs.IsDeleted = 0 AND 
					s.IsDeleted = 0
			 ) CourseSkills
    FROM Course c
	LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
	LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
	LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.CourseGuid = @CourseGuid
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration
2019.12.19 - Bill Koenig - Modifying image properties, added example, added course level to output to match other similar stored procedures
</remarks>
<description>
Retrieves a subscriber''s favorite courses
</description>
<example>
EXEC [dbo].[System_Get_Favorite_Courses] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9'', @Limit = 5, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_Favorite_Courses] (
    @SubscriberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    SELECT c.Name AS Title
        ,NULL AS Duration
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,c.TabletImage 
		,c.DesktopImage
		,c.MobileImage
		,c.ThumbnailImage
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid		
		,cl.Name as Level
		,c.Code 
    FROM Course c
    JOIN CourseFavorite cf ON c.CourseId = cf.CourseId
    JOIN Subscriber s on cf.SubscriberId = s.SubscriberId
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    WHERE s.SubscriberGuid = @SubscriberGuid AND cf.IsDeleted = 0
    ORDER BY 
    CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN c.Name END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''vendorName'' THEN v.Name END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN c.ModifyDate END,
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN c.Name  END desc,
    CASE WHEN @Sort = ''descending'' AND @Order = ''vendorName'' THEN v.Name END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN c.CreateDate END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN c.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END
            ')");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_RelatedCoursesBySkills]");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_CoursesForJob]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
