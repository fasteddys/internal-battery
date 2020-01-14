using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Courses_By_Topic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 migrationBuilder.Sql(@"Exec('
      /*
<remarks>
2020-01-09 - Jim Brazil - Created
2020-01-14
</remarks>
<description>
Returns course information with the name of the related topic 
</description>
<example>
SELECT * FROM [dbo].[v_CourseTopicView] ORDER BY ModifyDate DESC
</example>

*/
ALTER VIEW [dbo].[v_CourseTopicView] 
AS  
    SELECT 
	  tp.Name as Topic, tp.TopicGuid,  c.*
	 FROM 
	   tagtopic t
	   left join topic tp on tp.TopicId = t.TopicId
	   left join course c on t.topicId = c.TopicId or c.TopicSecondaryId = t.TopicId
	WHERE 
	   c.IsDeleted = 0
            ')");

            migrationBuilder.Sql(@"EXEC(' 
/*
<remarks>
2019.12.09 - Jim Brazil
 
</remarks>
<description>
Retrieves list of courses with filter options for the givenn topic
</description>
<example>
EXEC [dbo].[System_Get_Courses] @Topic = ''Data Science'' @Limit = 10, @Offset = 0, @Order = ''title'', @Sort = ''ascending''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByTopic] (
    @Topic UNIQUEIDENTIFIER,
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
    FROM v_coursetopicview c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
	WHERE
	    c.TopicGuid = @Topic
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
        }
        

        protected override void Down(MigrationBuilder migrationBuilder)
        {
 migrationBuilder.Sql(@"Exec('
      /*
<remarks>
2020-01-09 - Jim Brazil - Created
</remarks>
<description>
Returns course information with the name of the related topic 
</description>
<example>
SELECT * FROM [dbo].[v_CourseTopicView] ORDER BY ModifyDate DESC
</example>

*/
ALTER VIEW [dbo].[v_CourseTopicView] 
AS  
    SELECT 
	  tp.Name as Topic, c.*
	 FROM 
	   tagtopic t
	   left join topic tp on tp.TopicId = t.TopicId
	   left join course c on t.topicId = c.TopicId or c.TopicSecondaryId = t.TopicId
	WHERE 
	   c.IsDeleted = 0
            ')");

            migrationBuilder.Sql(@"EXEC(' 
/*
<remarks>
2019.12.09 - Jim Brazil
 
</remarks>
<description>
Retrieves list of courses with filter options for the givenn topic
</description>
<example>
EXEC [dbo].[System_Get_Courses] @Topic = ''Data Science'' @Limit = 10, @Offset = 0, @Order = ''title'', @Sort = ''ascending''
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CoursesByTopic] (
    @Topic varchar(max),
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
    FROM v_coursetopicview c
    LEFT JOIN Vendor v ON v.VendorId = c.VendorId
    LEFT JOIN CourseLevel cl ON cl.CourseLevelId = c.CourseLevelId
    LEFT JOIN Topic t on c.TopicId = t.TopicId
	WHERE
	    c.Topic = @Topic
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
        }
    }
}
