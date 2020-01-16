using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingtotalrecordstolistoutput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_Notifications] @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [dbo].[System_Get_Notifications] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT NotificationId
		FROM [Notification]
		WHERE IsDeleted = 0
	)
	SELECT 
		NotificationGuid,
		Title,
		[Description],
		CAST(IsTargeted AS BIT)  AS  IsTargeted,
		ExpirationDate,
		0 as HasRead,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM 
		[Notification]
	WHERE IsDeleted = 0					
	ORDER BY  
	CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN Title END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.26 - Jim Brazil - Created      
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
</remarks>
<description>
Returns subscriber notifications  
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotifications] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
ALTER PROCEDURE [dbo].[System_Get_SubscriberNotifications] (
    @SubscriberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT NotificationId
		FROM SubscriberNotification sn
		INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
		WHERE sn.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
	)
    SELECT 
    	sn.SubscriberNotificationGuid as NotificationGuid,
    	n.Title,
    	n.[Description],
    	CAST(n.IsTargeted AS BIT)  AS  IsTargeted,
    	n.ExpirationDate,
    	sn.HasRead as HasRead,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM 
    	SubscriberNotification sn
    	INNER JOIN [Notification] n on sn.NotificationId = n.NotificationId
    	INNER JOIN Subscriber s on sn.SubscriberId = s.SubscriberId
    WHERE sn.SubscriberId = s.SubscriberId and sn.IsDeleted = 0	and s.SubscriberGuid = @SubscriberGuid			
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN n.Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN n.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN n.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN n.Title END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN n.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN n.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Copied from old stored procedure for backward compatibility, fixed sort bug, added example
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_LegacyNotifications] @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_LegacyNotifications] ( 
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    SELECT 
    	NotificationGuid,
    	Title,
    	[Description],
    	CAST(IsTargeted AS BIT)  AS  IsTargeted,
    	ExpirationDate,
    	0 as HasRead,
    	CreateDate,
    	IsDeleted,
    	ModifyDate,
    	CreateGuid,
    	ModifyGuid
    FROM 
    	Notification
    WHERE IsDeleted = 0					
    ORDER BY  
	CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN Title END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.26 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Copied from old stored procedure for backward compatibility, fixed sort bug, added example
</remarks>
<description>
Returns subscriber notifications 
</description>
<example>
EXEC [dbo].[System_Get_LegacyNotifications] @Limit = 2, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_LegacySubscriberNotifications] (
    @SubscriberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    SELECT 
    	sn.SubscriberNotificationGuid as NotificationGuid,
    	n.Title,
    	n.[Description],
    	CAST(n.IsTargeted AS BIT)  AS  IsTargeted,
    	n.ExpirationDate,
    	sn.HasRead as HasRead,
    	n.CreateDate,
    	n.IsDeleted,
    	n.ModifyDate,
    	n.CreateGuid,
    	n.ModifyGuid
    FROM 
    	SubscriberNotification sn
    	LEFT JOIN Notification n on sn.NotificationId = n.NotificationId
    	LEFT JOIN Subscriber s on sn.SubscriberId = s.SubscriberId
    WHERE sn.SubscriberId = s.SubscriberId and sn.IsDeleted = 0	and s.SubscriberGuid =   @SubscriberGuid			
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN n.Title END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN n.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN n.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN n.Title END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN n.CreateDate END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''recruiter'' THEN n.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns companies
</description>
<example>
EXEC [dbo].[System_Get_Companies] @Limit = 12, @Offset = 10, @Sort = ''companyName'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_Companies] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT CompanyId
		FROM Company 
		WHERE IsDeleted = 0
	)
    SELECT CompanyGuid
    	, CompanyName
		, NULL AS JobPageBoilerplate
		, LogoUrl
		, IsHiringAgency
		, IsJobPoster
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Company 
    WHERE IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''companyName'' THEN CompanyName END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''companyName'' THEN CompanyName END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns topics
</description>
<example>
EXEC [dbo].[System_Get_Topics] @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_Topics] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT TopicId
		FROM Topic 
		WHERE IsDeleted = 0
	)
    SELECT TopicGuid
		, [Name]
		, [Description]
		, SortOrder
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Topic 
    WHERE IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns compensation types
</description>
<example>
EXEC [dbo].[System_Get_CompensationTypes] @Limit = 5, @Offset = 2, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_CompensationTypes] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT CompensationTypeId
		FROM CompensationType 
		WHERE IsDeleted = 0
	)
    SELECT CompensationTypeGuid
		, CompensationTypeName
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM CompensationType 
    WHERE IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''compensationTypeName'' THEN [CompensationTypeName] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''compensationTypeName'' THEN [CompensationTypeName] END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns compensation types
</description>
<example>
EXEC [dbo].[System_Get_Countries] @Limit = 100, @Offset = 0, @Sort = ''code2'', @Order = ''ascending''
</example> 
*/
CREATE PROCEDURE [dbo].[System_Get_Countries] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT CountryId
		FROM Country 
		WHERE IsDeleted = 0
	)
    SELECT CountryGuid
		, DisplayName
		, OfficialName
		, [Sequence]
		, Code2
		, Code3
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Country 
    WHERE IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''displayName'' THEN DisplayName END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''sequence'' THEN [Sequence] END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''code2'' THEN Code2 END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''code3'' THEN Code3 END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''displayName'' THEN DisplayName END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''sequence'' THEN [Sequence] END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''code2'' THEN Code2 END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''code3'' THEN Code3 END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration, includes changes to course level
2019.12.17 - Jim Brazil - Added support for additional course details being returned
2019.12.19 - Bill Koenig - Modifying image properties, added example, removed course variant
2019.12.30 - Jyoti Guin - Added ExternalUrl property
2020.01.14 - Bill Koenig - Adding support for TotalRecords, added logical delete to WHERE clause
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
	WITH allRecords AS (
		SELECT CourseId
		FROM Course 
		WHERE IsDeleted = 0
	)
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
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Course c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
    WHERE c.IsDeleted = 0
	ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN c.Name END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''vendorName'' THEN v.Name END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN c.Name  END desc,
    CASE WHEN @Order = ''descending'' AND @Sort = ''vendorName'' THEN v.Name END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN c.CreateDate END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019.12.09 - Bill Koenig - Added comment block for up migration
2019.12.19 - Bill Koenig - Modifying image properties, added example, added course level to output to match other similar stored procedures
2019.12.30 - Jyoti Guin - Added ExternalUrl property
2020.01.14 - Bill Koenig - Added support for TotalRecords
</remarks>
<description>
Retrieves a subscriber''s favorite courses
</description>
<example>
EXEC [dbo].[System_Get_Favorite_Courses] @SubscriberGuid = ''47568E38-A8D5-440E-B613-1C0C75787E90'', @Limit = 5, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
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
	WITH allRecords AS (
		SELECT CourseId
		FROM CourseFavorite cf
		INNER JOIN Subscriber s ON cf.SubscriberId = s.SubscriberId		 
		WHERE cf.IsDeleted = 0
		AND s.SubscriberGuid = @SubscriberGuid
	)
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
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Course c
    JOIN CourseFavorite cf ON c.CourseId = cf.CourseId
    JOIN Subscriber s on cf.SubscriberId = s.SubscriberId
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    WHERE s.SubscriberGuid = @SubscriberGuid AND cf.IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''title'' THEN c.Name END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''vendorName'' THEN v.Name END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN c.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''title'' THEN c.Name  END desc,
    CASE WHEN @Order = ''descending'' AND @Sort = ''vendorName'' THEN v.Name END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN c.CreateDate END desc ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN c.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Retrieves course levels
</description>
<example>
EXEC [dbo].[System_Get_CourseLevels] @Limit = 5, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_CourseLevels] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT CourseLevelId
		FROM CourseLevel 		
		WHERE IsDeleted = 0
	)
    SELECT CourseLevelGuid
        , [Name]
        , [Description]
        , SortOrder
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM CourseLevel
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name]  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.15 - Bill Koenig - Created
</remarks>
<description>
Retrieves education levels
</description>
<example>
EXEC [dbo].[System_Get_EducationLevels] @Limit = 10, @Offset = 0, @Sort = ''level'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_EducationLevels] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT EducationLevelId
		FROM EducationLevel 		
		WHERE IsDeleted = 0
	)
    SELECT EducationLevelGuid
        , [Level]        
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM EducationLevel
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''level'' THEN [Level] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''level'' THEN [Level]  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.15 - Bill Koenig - Created
</remarks>
<description>
Retrieves educational degree types
</description>
<example>
EXEC [dbo].[System_Get_EducationalDegreeTypes] @Limit = 22, @Offset = 0, @Sort = ''degreeType'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_EducationalDegreeTypes] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT EducationalDegreeTypeId
		FROM EducationalDegreeType 		
		WHERE IsDeleted = 0
	)
    SELECT EducationalDegreeTypeGuid
        , DegreeType        
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM EducationalDegreeType
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''degreeType'' THEN DegreeType END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''degreeType'' THEN DegreeType  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.15 - Bill Koenig - Created
</remarks>
<description>
Retrieves employment types
</description>
<example>
EXEC [dbo].[System_Get_EmploymentTypes] @Limit = 10, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_EmploymentTypes] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT EmploymentTypeId
		FROM EmploymentType 		
		WHERE IsDeleted = 0
	)
    SELECT EmploymentTypeGuid
        , [Name]        
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM EmploymentType
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name]  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.15 - Bill Koenig - Created
</remarks>
<description>
Retrieves experience levels
</description>
<example>
EXEC [dbo].[System_Get_ExperienceLevels] @Limit = 10, @Offset = 0, @Sort = ''code'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_ExperienceLevels] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT ExperienceLevelId
		FROM ExperienceLevel 		
		WHERE IsDeleted = 0
	)
    SELECT ExperienceLevelGuid
        , DisplayName
		, Code   
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM ExperienceLevel
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''displayName'' THEN DisplayName END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''code'' THEN Code END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''displayName'' THEN DisplayName  END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''code'' THEN Code  END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Retrieves industries
</description>
<example>
EXEC [dbo].[System_Get_Industries] @Limit = 10, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Industries] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT IndustryId
		FROM Industry 		
		WHERE IsDeleted = 0
	)
    SELECT IndustryGuid
        , [Name]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Industry
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Retrieves offers
</description>
<example>
EXEC [dbo].[System_Get_Offers] @Limit = 10, @Offset = 0, @Sort = ''partnerName'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Offers] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT OfferId
		FROM Offer 		
		WHERE IsDeleted = 0
	)
    SELECT OfferGuid
        , o.[Name]
		, o.[Description]
		, Disclaimer
		, Code
		, [Url]
		, p.PartnerGuid
		, p.LogoUrl [PartnerLogoUrl]
		, p.[Name] PartnerName
		, StartDate
		, EndDate
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Offer o
	INNER JOIN [Partner] p ON o.PartnerId = p.PartnerId
    WHERE o.IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN o.[Name] END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''code'' THEN o.Code END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''partnerName'' THEN p.[Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN o.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN o.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN o.[Name] END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''code'' THEN o.Code END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''partnerName'' THEN p.[Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN o.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN o.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Retrieves security clearances
</description>
<example>
EXEC [dbo].[System_Get_SecurityClearances] @Limit = 4, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_SecurityClearances] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT SecurityClearanceId
		FROM SecurityClearance 		
		WHERE IsDeleted = 0
	)
    SELECT SecurityClearanceGuid
        , [Name]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM SecurityClearance
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN [Name] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN [Name] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Retrieves skills
</description>
<example>
EXEC [dbo].[System_Get_Skills] @Limit = 4, @Offset = 0, @Sort = ''name'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_Skills] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT SkillId
		FROM Skill 		
		WHERE IsDeleted = 0
	)
    SELECT SkillGuid
        , SkillName [Name]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Skill
    WHERE IsDeleted = 0
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN SkillName END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN SkillName END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Retrieves states for a country
</description>
<example>
EXEC [dbo].[System_Get_States] @Country = ''8B5DEC9A-B5CF-4BDC-B015-CCFD4339D32B'', @Limit = 10, @Offset = 0, @Sort = ''sequence'', @Order = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_States] (
	@Country uniqueidentifier,
	@Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT StateId
		FROM [State] s 		
		INNER JOIN Country c ON s.CountryId = c.CountryId
		WHERE s.IsDeleted = 0
		AND c.CountryGuid = @Country
	)
    SELECT StateGuid
        , [Name]
		, Code
		, s.[Sequence]
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM [State] s 		
	INNER JOIN Country c ON s.CountryId = c.CountryId
    WHERE s.IsDeleted = 0
	AND c.CountryGuid = @Country
    ORDER BY 
    CASE WHEN @Order = ''ascending'' AND @Sort = ''name'' THEN s.[Name] END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''code'' THEN Code END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''sequence'' THEN s.[Sequence] END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN s.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN s.ModifyDate END,
    CASE WHEN @Order = ''descending'' AND @Sort = ''name'' THEN s.[Name] END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''code'' THEN Code END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''sequence'' THEN s.[Sequence] END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN s.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN s.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
2020.01.16 - Bill Koenig - Added support for total records, fixed sort and order, added example
</remarks>
<description>
Returns notes for subscriber 
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotes] @SubscriberGuid = ''df7a8931-c99b-40a0-b117-230a203db400'', @TalentGuid = ''47568e38-a8d5-440e-b613-1c0c75787e90'', @Limit = 10, @Offset = 0, @Sort = ''recruiter'', @Order = ''ascending''
</example>
 */
ALTER PROCEDURE [dbo].[System_Get_SubscriberNotes] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @TalentGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT sn.SubscriberNotesId
		FROM SubscriberNotes sn
		INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
		INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
		WHERE rs.SubscriberGuid = @SubscriberGuid 
		AND s.SubscriberGuid = @TalentGuid  
		AND sn.IsDeleted = 0  
	)
    SELECT sn.SubscriberNotesGuid, 
		s.SubscriberGuid,
		r.RecruiterGuid,
		r.FirstName + '' '' + r.LastName RecruiterName,
		sn.Notes,
		sn.ViewableByOthersInRecruiterCompany,
		s.CreateDate,
		s.ModifyDate as ModifiedDate,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM SubscriberNotes sn
	INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
	INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
	INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
	WHERE rs.SubscriberGuid = @SubscriberGuid 
	AND s.SubscriberGuid = @TalentGuid   
	AND sn.IsDeleted = 0   
	ORDER BY  
	CASE WHEN @Order = ''ascending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN sn.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN sn.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2020.01.16 - Bill Koenig - Created
</remarks>
<description>
Returns talent favorites
</description>
<example>
EXEC [dbo].[System_Get_TalentFavorites] @SubscriberGuid = ''DF7A8931-C99B-40A0-B117-230A203DB400'', @Limit = 4, @Offset = 0, @Sort = ''createDate'', @Order = ''descending''
</example>
 */
CREATE PROCEDURE [dbo].[System_Get_TalentFavorites] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT tf.TalentFavoriteId
		FROM TalentFavorite tf
		INNER JOIN Subscriber s ON tf.SubscriberId = s.SubscriberId
		INNER JOIN Subscriber t ON tf.TalentId = t.SubscriberId
		WHERE s.SubscriberGuid = @SubscriberGuid   
		AND tf.IsDeleted = 0  
	)
    SELECT t.SubscriberGuid
		, t.FirstName
		, t.LastName
		, t.Email
		, t.PhoneNumber
		, t.ProfileImage
		, t.CreateDate [JoinDate]
		, t.ModifyDate
		, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM TalentFavorite tf
	INNER JOIN Subscriber s ON tf.SubscriberId = s.SubscriberId
	INNER JOIN Subscriber t ON tf.TalentId = t.SubscriberId
	WHERE s.SubscriberGuid = @SubscriberGuid   
	AND tf.IsDeleted = 0   
	ORDER BY  

    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN t.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN t.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN t.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN t.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}