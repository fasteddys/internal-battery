using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedtocoursesbytopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019.12.09 - Jim Brazil - Created
2020.01.17 - Bill Koenig - Fixed sort and order logic, added support for total records, updated example
</remarks>
<description>
Retrieves list of courses with filter options for the given topic
</description>
<example>
EXEC [dbo].[System_Get_CoursesByTopic] @Topic = ''78524787-F420-4129-A14F-DF6F3C902C3B'', @Limit = 10, @Offset = 0, @Sort = ''title'', @Order = ''ascending''
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
	WITH allRecords AS (
		SELECT CourseId
		FROM v_CourseTopicView
		WHERE TopicGuid = @Topic
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
    CASE WHEN @Sort = ''descending'' AND @Order = ''title'' THEN c.Name  END DESC,
    CASE WHEN @Sort = ''descending'' AND @Order = ''vendorName'' THEN v.Name END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN c.CreateDate END DESC ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN c.ModifyDate END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
