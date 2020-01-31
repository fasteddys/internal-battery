using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingcoursetopicview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
CREATE VIEW [dbo].[v_CourseTopicView] 
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

        }

 

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"DROP VIEW v_CourseTopicView");

        }
    }
}
