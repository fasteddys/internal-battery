using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class v_CourseTopicView : Migration
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
    SELECT topic.Name AS Topic
	,Topic.TopicGuid
	,c.*
FROM TagCourse tc
LEFT JOIN tag ta ON tc.TagId = ta.TagId
LEFT JOIN TagTopic tt ON tt.TagId = ta.TagId
LEFT JOIN Topic ON Topic.topicId = tt.topicId
LEFT JOIN Course c ON c.CourseId = tc.courseId
WHERE c.IsDeleted = 0
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
