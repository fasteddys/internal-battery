using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class coursesprocsandcourseleveldata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
</remarks>
<description>
Retrieves courses with filter options
</description>
*/
CREATE PROCEDURE [dbo].[System_Get_Courses] (
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

            migrationBuilder.Sql(@"EXEC('
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

            migrationBuilder.Sql(@"EXEC('
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

            migrationBuilder.Sql(@"EXEC('
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

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration
</remarks>
<description>
Retrieves a subscriber''s favorite courses
</description>
*/
CREATE PROCEDURE [dbo].[System_Get_Favorite_Courses] (
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
        ,c.TabletImage as ThumbnailUrl
        ,v.VendorGuid
        ,cl.CourseLevelGuid as CourseLevelGuid
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

            migrationBuilder.InsertData(
                table: "CourseLevel",
                columns: new[] { "IsDeleted", "CreateDate", "CreateGuid", "CourseLevelGuid", "Name", "Description", "SortOrder" },
                values: new object[] { 0, DateTime.UtcNow, Guid.Empty, "B70F8621-DD0C-4C10-A67E-6ED68752CECB", "Beginner", "Course has beginner level of difficulty", 10 }
                );

            migrationBuilder.InsertData(
                table: "CourseLevel",
                columns: new[] { "IsDeleted", "CreateDate", "CreateGuid", "CourseLevelGuid", "Name", "Description", "SortOrder" },
                values: new object[] { 0, DateTime.UtcNow, Guid.Empty, "663553E1-E1BE-4D79-B8A9-6CC4C83C9697", "Intermediate", "Course has intermediate level of difficulty", 20 }
                );

            migrationBuilder.InsertData(
                table: "CourseLevel",
                columns: new[] { "IsDeleted", "CreateDate", "CreateGuid", "CourseLevelGuid", "Name", "Description", "SortOrder" },
                values: new object[] { 0, DateTime.UtcNow, Guid.Empty, "E3FABB83-196F-4E5C-8F5D-90351CC4DCC7", "Advanced", "Course has advanced level of difficulty", 30 }
                );

            migrationBuilder.Sql(@"
UPDATE 
	c 
SET
	c.TabletImage = CONVERT(VARCHAR(MAX), CourseGuid) + '/Thumbnail.png'
	, c.ModifyDate = GETUTCDATE()
	, c.ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM Course c
INNER JOIN Vendor v ON c.VendorId = v.VendorId
WHERE v.Name = 'WozU'
            ");
                                          
            migrationBuilder.Sql("UPDATE [dbo].[Vendor] SET LogoUrl = '00000000-0000-0000-0000-000000000001/woz.png', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE [Name] = 'WozU'");

            migrationBuilder.Sql(@"
;WITH courseLevelMappings AS (
	SELECT *
	FROM (
		VALUES ('Coding From Scratch', 'Full Stack', 'Beginner')
		, ('Cryptography and Access Management', 'Cyber Security', 'Advanced')
		, ('Network Defense', 'Cyber Security', 'Intermediate')
		, ('System Administration', 'Cyber Security', 'Beginner')
		, ('Networking Foundations', 'Cyber Security', 'Beginner')
		, ('Security Foundations', 'Cyber Security', 'Beginner')
		, ('Programming Foundations - Python', 'Data Science', 'Beginner')
		, ('Introduction to Big Data', 'Data Science', 'Beginner')
		, ('Modeling and Optimization', 'Data Science', 'Advanced')
		, ('Machine Learning', 'Data Science', 'Advanced')
		, ('Intermediate Statistics', 'Data Science', 'Intermediate')
		, ('Data Wrangling and Visualization', 'Data Science', 'Beginner')
		, ('Metrics and Data Processing', 'Data Science', 'Intermediate')
		, ('Statistical Programming in R', 'Data Science', 'Beginner')
		, ('Basic Statistics', 'Data Science', 'Beginner')
		, ('Web Security Foundations', 'Full Stack', 'Intermediate')
		, ('Deployment', 'Full Stack', 'Intermediate')
		, ('Agile Project Management', 'Full Stack', 'Beginner')
		, ('Responsive Web Design', 'Full Stack', 'Intermediate')
		, ('Database', 'Full Stack', 'Beginner')
		, ('Back End Foundations - Java', 'Full Stack', 'Intermediate')
		, ('Back End Foundations - JavaScript', 'Full Stack', 'Intermediate')
		, ('Back End Foundations - C#', 'Full Stack', 'Intermediate')
		, ('Front End Frameworks - React', 'Full Stack', 'Advanced')
		, ('Front End Frameworks - Angular', 'Full Stack', 'Advanced')
		, ('Programming Foundations Java', 'Full Stack', 'Intermediate')
		, ('Programming Foundations C#', 'Full Stack', 'Intermediate')
		, ('Front End Foundations', 'Full Stack', 'Intermediate ')
		, ('Logging and Monitoring', 'Cyber Security', 'Advanced')
		, ('Threats and Vulnerabilities', 'Cyber Security', 'Intermediate')
	) CourseLevelMapping(CourseName, TopicName, CourseLevelName)
)
UPDATE
	c
SET
	c.CourseLevelId = cl.CourseLevelId
	, c.ModifyDate = GETUTCDATE()
	, c.ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM 
	courseLevelMappings clm
	INNER JOIN Course c ON clm.CourseName = c.[Name]
	INNER JOIN Topic t ON c.TopicId = t.TopicId
	INNER JOIN Vendor v ON c.VendorId = v.VendorId
	INNER JOIN CourseLevel cl ON clm.CourseLevelName = cl.[Name]
WHERE 
	v.[Name] = 'WozU'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_Courses]");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_Course]");
            
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.23 - Jim Brazil - Created 
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
		0  as NewFlag,
		v.Name as VendorName,
		c.CourseGuid,
		0 as CourseLevel,	
		0 as NumLessons,
		null as VendorLogoUrl,
		v.VendorGuid
FROM CourseSkill cs
	left join Skill s on s.SkillId = cs.SkillId
	left join Course c on cs.CourseId = c.CourseId
	left join Vendor v on v.VendorId = c.VendorId
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

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.10.24 - Jim Brazil - Created 
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
	Select  top(@MaxResults)
	c.Name as Title ,
		null as Duration,
		c.Description, 
		(select count(*) from Enrollment where CourseId = c.CourseId) as NumEnrollments ,
		0  as NewFlag,
		v.Name as VendorName,
		c.CourseGuid,
		0 as CourseLevel,	
		0 as NumLessons,
		null as VendorLogoUrl,
		v.VendorGuid
FROM  Course c  
	left join Vendor v on v.VendorId = c.VendorId
 
ORDER by newid()          
END
            ')");

            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_Favorite_Courses]");
                        
            migrationBuilder.Sql("UPDATE [dbo].[Course] SET CourseLevelId = NULL, TabletImage = NULL, ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000'");
            
            migrationBuilder.Sql("UPDATE [dbo].[Vendor] SET LogoUrl = NULL, ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE [Name] = 'WozU'");

            migrationBuilder.Sql("DELETE FROM [dbo].[CourseLevel]");
        }
    }
}
