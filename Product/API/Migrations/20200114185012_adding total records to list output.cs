using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingtotalrecordstolistoutput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_Notifications] @Limit = 2, @Offset = 0, @Sort = ''ascending'', @Order = ''title''
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
	CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN Title END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN Title END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
	CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>
2019.12.26 - Jim Brazil - Created      
2020.01.13 - Bill Koenig - Added example, fixed order by logic, added total records count, removed unnecessary columns
</remarks>
<description>
Returns subscriber notifications  
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotifications] @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 2, @Offset = 0, @Sort = ''ascending'', @Order = ''title''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN n.Title END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN n.CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN n.ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN n.Title END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN n.CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN n.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>
2019.12.23 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Copied from old stored procedure for backward compatibility, fixed sort bug, added example
</remarks>
<description>
Returns system notifications
</description>
<example>
EXEC [dbo].[System_Get_LegacyNotifications] @Limit = 2, @Offset = 0, @Sort = ''ascending'', @Order = ''title''
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
	CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN Title END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN Title END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
	CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>
2019.12.26 - Jim Brazil - Created 
2020.01.13 - Bill Koenig - Copied from old stored procedure for backward compatibility, fixed sort bug, added example
</remarks>
<description>
Returns subscriber notifications 
</description>
<example>
EXEC [dbo].[System_Get_LegacyNotifications] @Limit = 2, @Offset = 0, @Sort = ''ascending'', @Order = ''title''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''title'' THEN n.Title END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN n.CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN n.ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN n.Title END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN n.CreateDate END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''recruiter'' THEN n.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns companies
</description>
<example>
EXEC [dbo].[System_Get_Companies] @Limit = 12, @Offset = 10, @Sort = ''ascending'', @Order = ''companyName''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''companyName'' THEN CompanyName END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''companyName'' THEN CompanyName END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns topics
</description>
<example>
EXEC [dbo].[System_Get_Topics] @Limit = 10, @Offset = 0, @Sort = ''descending'', @Order = ''modifyDate''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''name'' THEN [Name] END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''name'' THEN [Name] END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns compensation types
</description>
<example>
EXEC [dbo].[System_Get_CompensationTypes] @Limit = 5, @Offset = 2, @Sort = ''descending'', @Order = ''modifyDate''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''compensationTypeName'' THEN [CompensationTypeName] END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''compensationTypeName'' THEN [CompensationTypeName] END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>     
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Returns compensation types
</description>
<example>
EXEC [dbo].[System_Get_Countries] @Limit = 100, @Offset = 0, @Sort = ''descending'', @Order = ''code2''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''displayName'' THEN DisplayName END,
	CASE WHEN @Sort = ''ascending'' AND @Order = ''sequence'' THEN [Sequence] END,
	CASE WHEN @Sort = ''ascending'' AND @Order = ''code2'' THEN Code2 END,
	CASE WHEN @Sort = ''ascending'' AND @Order = ''code3'' THEN Code3 END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END, 
    CASE WHEN @Sort = ''descending'' AND @Order = ''displayName'' THEN DisplayName END DESC ,
	CASE WHEN @Sort = ''descending'' AND @Order = ''sequence'' THEN [Sequence] END DESC ,
	CASE WHEN @Sort = ''descending'' AND @Order = ''code2'' THEN Code2 END DESC ,
	CASE WHEN @Sort = ''descending'' AND @Order = ''code3'' THEN Code3 END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('
/*
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
END')");

            migrationBuilder.Sql(@"EXEC('
/*
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
END')");

            migrationBuilder.Sql(@"EXEC('

/*
<remarks>
2020.01.14 - Bill Koenig - Created
</remarks>
<description>
Retrieves course levels
</description>
<example>
EXEC [dbo].[System_Get_CourseLevels] @Limit = 5, @Offset = 0, @Sort = ''ascending'', @Order = ''name''
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
    CASE WHEN @Sort = ''ascending'' AND @Order = ''name'' THEN [Name] END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN ModifyDate END,
    CASE WHEN @Sort = ''descending'' AND @Order = ''name'' THEN [Name]  END DESC,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
