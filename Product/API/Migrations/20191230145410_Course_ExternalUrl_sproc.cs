using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Course_ExternalUrl_sproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"Exec('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
        ,c.ExternalUrl
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

   migrationBuilder.Sql(@"
Exec('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
        ,c.ExternalUrl
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

   migrationBuilder.Sql(@"Exec('

ALTER PROCEDURE [dbo].[System_Get_CoursesByGuid] (
    @CourseGuid UNIQUEIDENTIFIER
)
AS
BEGIN 
    SELECT TOP 1 c.Name AS Title
        ,c.Description
        ,(
            SELECT count(*)
            FROM Enrollment
            WHERE CourseId = c.CourseId
            ) AS NumEnrollments
        ,v.Name AS VendorName
        ,c.CourseGuid
        ,v.LogoUrl AS VendorLogoUrl
        ,c.TabletImage as ThumbnailUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
        ,c.ExternalUrl
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    WHERE c.CourseGuid = @CourseGuid
END 

')");   

   migrationBuilder.Sql(@"
Exec('
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
2019.12.19 - Bill Koenig - Modifying image properties
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
    SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, c.ExternalUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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
Exec('
            
/*
<remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
2019.12.19 - Bill Koenig - Modifying image properties
2019.12.30 - Jyoti Guin - Added ExternalUrl property
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
    SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, c.ExternalUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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

   migrationBuilder.Sql(@"
Exec('


/* <remarks>
2019.12.06 - Bill Koenig - Created
2019.12.10 - Bill Koenig - Optimized based on performance observed in staging environment (removed unnecessary tables and columns from CTE expressions, added indexes to prevent table 
scans, and delayed joins on related job posting tables until scoring is complete)
2019.12.19 - Bill Koenig - Modifying image properties
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
    SELECT CourseGuid, c.[Name] CourseName, c.[Description] CourseDescription, c.TabletImage, c.DesktopImage, c.MobileImage, c.ThumbnailImage, c.ExternalUrl, v.[Name] VendorName, v.LogoUrl VendorLogoUrl, WeightedSkillScore
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

   migrationBuilder.Sql(@"
Exec('
            

/*
<remarks>
2019.10.24 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level, cleaned up formatting
2019.12.17 - Jim Brazil - Added support for additional course details being returned 
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
        ,c.ExternalUrl
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

   migrationBuilder.Sql(@"
Exec('

/*
<remarks>
2019.12.17 - Jim Brazil - Created
2019.12.19 - Bill Koenig - Modifying image properties, adding example
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
        c.ExternalUrl,
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

   migrationBuilder.Sql(@"

 Exec('          


/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration
2019.12.19 - Bill Koenig - Modifying image properties, added example, added course level to output to match other similar stored procedures
2019.12.30 - Jyoti Guin - Added ExternalUrl property

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
        ,c.ExternalUrl
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

   migrationBuilder.Sql(@"
Exec('


   /*     
<remarks>
2019.12.02 - Jim Brazil - Created 
2019.12.23 - Jim Brazil - Updated to NVL grade and also return enrollment status
2019.12.27 - Jim Brazil - Updated to include course vendor url 
2019.12.30 - Jyoti Guin - Added ExternalUrl property

</remarks>
<description>
Returns courses for subscriber 
</description>
 
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberCourses] (
            @SubscriberGuid UNIQUEIDENTIFIER,
            @ExcludeCompleted int,
            @ExcludeActive int

        )
        AS
BEGIN 

SELECT 
    ISNULL(e.grade,0) as Grade, 
    e.PercentComplete,
    c.Name as Title,
    c.Description,
    cl.Name as CourseLevel,
    0 as NumLessons,
    v.Name as VendorName,
    v.VendorGuid,
    v.LogoUrl as VendorLogoUrl,
    c.ExternalUrl,
    e.EnrollmentStatusId,
    ISNULL(vsl.RegistrationUrl,'''') as VendorCourseUrl

FROM 
Enrollment e
join Subscriber s on e.SubscriberId = s.SubscriberId
Join Course c on e.CourseId = c.CourseId
Join Vendor v on c.VendorId = v.vendorId
join CourseLevel cl on cl.CourseLevelId = c.CourseLevelId
left join VendorStudentLogin vsl on vsl.SubscriberId = s.SubscriberId and vsl.VendorId = v.VendorId
WHERE
e.IsDeleted = 0 and  
(   
   ( @ExcludeCompleted = 0 and @ExcludeActive = 0 and 1 = 1  ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 0 and e.PercentComplete < 100 ) OR
   ( @ExcludeCompleted = 0 and @ExcludeActive = 1 and e.PercentComplete = 100 ) OR
   ( @ExcludeCompleted = 1 and @ExcludeActive = 1 and 1 = 0 ) 
)

and s.SubscriberGuid = @SubscriberGuid
Order by e.ModifyDate desc       
END
 
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
