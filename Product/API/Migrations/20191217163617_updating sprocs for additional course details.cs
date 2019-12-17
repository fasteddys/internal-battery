using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatingsprocsforadditionalcoursedetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"Exec('           
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.17.09 - Jim Brazil added support for additional course details being returned 
</remarks>
<description>
Retrieves course by course identifier
</description>
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
        ,c.TabletImage as ThumbnailUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
		,cv.CourseVariantGuid
		,cv.Price
		,cvt.Name as CourseVariantType
		,c.Code
		,cl.Name as Level
		,c.CreateDate
		,c.ModifyDate
		,c.IsDeleted
		,c.TabletImage
		,c.DesktopImage
		,c.MobileImage
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
	LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
	LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
	LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.CourseGuid = @CourseGuid
END 
            ')");

            migrationBuilder.Sql(@"Exec('           

/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.17.09 - Jim Brazil added support for additional course details being returned 
</remarks>
<description>
Retrieves courses with filter options
</description>
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
        ,c.TabletImage as ThumbnailUrl
        ,cl.CourseLevelGuid as CourseLevelGuid
		,cv.CourseVariantGuid
		,cv.Price
		,cvt.Name as CourseVariantType
		,c.Code
		,cl.Name as Level
		,c.CreateDate
		,c.ModifyDate
		,c.IsDeleted
		,c.TabletImage
		,c.DesktopImage
		,c.MobileImage
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
	LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
	LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId
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
 /*
     <remarks>
    2019.10.23 - Bill Koenig - Created
	2019.17.09 - Jim Brazil added support for additional course details being returned 
    </remarks>
    <description>
    Retrieves a list of courses that are associated with the skills provided in the histogram. Results are
    </description>
    <example>
    DECLARE @SkillHistogram AS [dbo].[SkillHistogram]
    INSERT INTO @SkillHistogram
    SELECT Skill, Occurrences
    FROM (VALUES 
        (''authentication'', 10), 
        (''ruby'', 3), 
        (''python'', 2), 
        (''asp.net'', 1), 
        (''c#.net'', 1), 
        (''css'', 1), 
        (''html'', 1), 
        (''javascript'', 1), 
        (''json'', 1), 
        (''nodejs'', 1), 
        (''npm'', 1), 
        (''sql'', 1), 
        (''ssis'', 1), 
        (''ssrs'', 1), 
        (''xml'', 1)) AS SkillHistogram(Skill, Occurrences)
    EXEC [dbo].[System_Get_RelatedCoursesBySkills] @SkillHistogram = @SkillHistogram, @MaxResults = 5
    </example>
    */
    ALTER PROCEDURE [dbo].[System_Get_RelatedCoursesBySkills] (
        @SkillHistogram dbo.SkillHistogram READONLY
        , @MaxResults INT = NULL 
    )
    AS
    BEGIN
        SELECT TOP (ISNULL(@MaxResults, 2147483647)) 
            c.CourseGuid, 
    		c.Description,
            c.[Name] [Title], 
            0 [LessonCount], 
            0 [CourseLevel],         
            v.VendorGuid, 
            NULL [VendorLogoUrl], 
            SUM(sh.Occurrences)  OVER ( Partition BY  c.CourseGuid, c.CourseId,v.Name, c.[Name], v.VendorGuid, c.Description) as [Rank],
    		null as Duration,
    		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments,				
    		0  as NewFlag,
    		v.Name as VendorName,				 
    		0 as NumLessons,
			c.TabletImage as ThumbnailUrl
			,cl.CourseLevelGuid as CourseLevelGuid
			,cv.CourseVariantGuid
			,cv.Price
			,cvt.Name as CourseVariantType
			,c.Code
			,cl.Name as Level
			,c.CreateDate
			,c.ModifyDate
			,c.IsDeleted
			,c.TabletImage
			,c.DesktopImage
			,c.MobileImage
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
        FROM Course c WITH(NOLOCK)
        INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
        INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
        INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
        INNER JOIN @SkillHistogram sh ON s.SkillName = sh.Skill
	    LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
		LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId    
        LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
	    LEFT JOIN Topic t on c.TopicId = t.TopicId
 
        ORDER BY [Rank] DESC 
    END

')");
            migrationBuilder.Sql(@"Exec('
 
/*
<remarks>
2019.10.23 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level
2019.17.09 - Jim Brazil added support for additional course details being returned 
</remarks>
<description>
Retrieves courses by a job identifier using skills. This will be deprecated once we
begin using the entity related endpoints.
</description>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesForJob] (
    @JobGuid UniqueIdentifier,
	@MaxResults INT
)
AS
BEGIN 
	Select  top(@MaxResults)
	c.Name as Title ,
		null as Duration,
		c.Description, 
		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
		v.Name as VendorName,
		c.CourseGuid,
		v.LogoUrl as VendorLogoUrl,
        c.TabletImage as ThumbnailUrl,
		v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid 
			,cv.CourseVariantGuid
			,cv.Price
			,cvt.Name as CourseVariantType
			,c.Code
			,cl.Name as Level
			,c.CreateDate
			,c.ModifyDate
			,c.IsDeleted
			,c.TabletImage
			,c.DesktopImage
			,c.MobileImage
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

FROM CourseSkill cs
	left join Skill s on s.SkillId = cs.SkillId
	left join Course c on cs.CourseId = c.CourseId
	left join Vendor v on v.VendorId = c.VendorId
    left join CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
	LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
	LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId
    LEFT JOIN Topic t on c.TopicId = t.TopicId


WHERE s.SkillName in 
	(SELECT 
		s.SkillName 
	FROM 
		jobpostingskill jps
		left join skill s on jps.SkillId = s.SkillId
	WHERE 
		JobPostingId = (select JobPostingId from JobPosting where JobPostingGuid =  @JobGuid )
	) AND
		c.IsDeleted = 0
ORDER by NumEnrollments DESC           
END

')");


            migrationBuilder.Sql(@"Exec('
/*
<remarks>
2019.10.24 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level, cleaned up formatting
2019.17.09 - Jim Brazil added support for additional course details being returned 
</remarks>
<description>
Select random courses 
</description>
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
        c.TabletImage as ThumbnailUrl,
		v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid
		,cv.CourseVariantGuid
		,cv.Price
		,cvt.Name as CourseVariantType
		,c.Code
		,cl.Name as Level
		,c.CreateDate
		,c.ModifyDate
		,c.IsDeleted
		,c.TabletImage
		,c.DesktopImage
		,c.MobileImage
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
	    LEFT JOIN CourseVariant cv on cv.CourseId = c.CourseId
		LEFT JOIN CourseVariantType cvt on cv.CourseVariantTypeId = cv.CourseVariantTypeId    
		LEFT JOIN Topic t on c.TopicId = t.TopicId
    ORDER by newid()          
END

')");





        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {



            migrationBuilder.Sql(@"Exec('
    /*
    <remarks>
    2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
    </remarks>
    <description>
    Retrieves course by course identifier
    </description>
    */
    CREATE PROCEDURE [dbo].[System_Get_Course] (
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
            ,c.TabletImage as ThumbnailUrl
            ,v.VendorGuid
            ,cl.CourseLevelGuid as CourseLevelGuid
        FROM Course c
        LEFT JOIN Vendor v ON v.VendorId = c.VendorId
        LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
        WHERE c.CourseGuid = @CourseGuid
    END
            ')");

 

            migrationBuilder.Sql(@"Exec('

/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
</remarks>
<description>
Retrieves courses with filter options
</description>
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
        ,c.TabletImage as ThumbnailUrl
        ,cl.CourseLevelGuid as CourseLevelGuid
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
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
/*
     <remarks>
    2019.10.23 - Bill Koenig - Created
    </remarks>
    <description>
    Retrieves a list of courses that are associated with the skills provided in the histogram. Results are
    </description>
    <example>
    DECLARE @SkillHistogram AS [dbo].[SkillHistogram]
    INSERT INTO @SkillHistogram
    SELECT Skill, Occurrences
    FROM (VALUES 
        (''authentication'', 10), 
        (''ruby'', 3), 
        (''python'', 2), 
        (''asp.net'', 1), 
        (''c#.net'', 1), 
        (''css'', 1), 
        (''html'', 1), 
        (''javascript'', 1), 
        (''json'', 1), 
        (''nodejs'', 1), 
        (''npm'', 1), 
        (''sql'', 1), 
        (''ssis'', 1), 
        (''ssrs'', 1), 
        (''xml'', 1)) AS SkillHistogram(Skill, Occurrences)
    EXEC [dbo].[System_Get_RelatedCoursesBySkills] @SkillHistogram = @SkillHistogram, @MaxResults = 5
    </example>
    */
    ALTER PROCEDURE [dbo].[System_Get_RelatedCoursesBySkills] (
        @SkillHistogram dbo.SkillHistogram READONLY
        , @MaxResults INT = NULL 
    )
    AS
    BEGIN
        SELECT TOP (ISNULL(@MaxResults, 2147483647)) 
            c.CourseGuid, 
    		c.Description,
            c.[Name] [Title], 
            0 [LessonCount], 
            0 [CourseLevel],         
            v.VendorGuid, 
            NULL [VendorLogoUrl], 
            SUM(sh.Occurrences) [Rank], 
    		null as Duration,
    		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments,				
    		0  as NewFlag,
    		v.Name as VendorName,				 
    		0 as NumLessons 
        FROM Course c WITH(NOLOCK)
        INNER JOIN Vendor v WITH(NOLOCK) ON c.VendorId = v.VendorId
        INNER JOIN CourseSkill cs WITH(NOLOCK) ON c.CourseId = cs.CourseId
        INNER JOIN Skill s WITH(NOLOCK) ON cs.SkillId = s.SkillId
        INNER JOIN @SkillHistogram sh ON s.SkillName = sh.Skill
        GROUP BY c.CourseGuid, c.CourseId,v.Name, c.[Name], v.VendorGuid, c.Description
        ORDER BY [Rank] DESC 
    END
    


        ')");



            migrationBuilder.Sql(@"Exec('

/*
<remarks>
2019.10.23 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level
</remarks>
<description>
Retrieves courses by a job identifier using skills. This will be deprecated once we
begin using the entity related endpoints.
</description>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesForJob] (
    @JobGuid UniqueIdentifier,
	@MaxResults INT
)
AS
BEGIN 
	Select  top(@MaxResults)
	c.Name as Title ,
		null as Duration,
		c.Description, 
		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
		v.Name as VendorName,
		c.CourseGuid,
		v.LogoUrl as VendorLogoUrl,
        c.TabletImage as ThumbnailUrl,
		v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid

FROM CourseSkill cs
	left join Skill s on s.SkillId = cs.SkillId
	left join Course c on cs.CourseId = c.CourseId
	left join Vendor v on v.VendorId = c.VendorId
    left join CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
WHERE s.SkillName in 
	(SELECT 
		s.SkillName 
	FROM 
		jobpostingskill jps
		left join skill s on jps.SkillId = s.SkillId
	WHERE 
		JobPostingId = (select JobPostingId from JobPosting where JobPostingGuid =  @JobGuid )
	) AND
		c.IsDeleted = 0
ORDER by NumEnrollments DESC           
END

  ')");


            migrationBuilder.Sql(@"Exec('

/*
<remarks>
2019.10.24 - Jim Brazil - Created 
2019.12.09 - Bill Koenig - Includes changes to course level, cleaned up formatting
</remarks>
<description>
Select random courses 
</description>
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
        c.TabletImage as ThumbnailUrl,
		v.VendorGuid,
        cl.CourseLevelGuid as CourseLevelGuid
    FROM  Course c  
    	LEFT JOIN Vendor v ON v.VendorId = c.VendorId
        LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    ORDER by newid()          
END

  ')");

        }
    }
}
