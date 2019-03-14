using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedv_SubscriberSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019-03-14 - Bill Koenig - Created
</remarks>
<description>
Returns subscriber sources aggregated with counts for number of subscribers
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberSources]
</example>
*/
CREATE VIEW [dbo].[v_SubscriberSources]
AS
	
	WITH subscriberSources AS (
		SELECT s.SubscriberId
			, ISNULL(JSON_VALUE(ProfileData, ''$.source''), ''None'') [Source]
			, ISNULL(JSON_VALUE(ProfileData, ''$.referer''), ''None'') [Referrer]
		FROM Subscriber s WITH(NOLOCK)
		LEFT JOIN SubscriberProfileStagingStore spss WITH(NOLOCK) ON s.SubscriberId = spss.SubscriberId 
			-- placing this logic in the join intentionally so that subscribers without sources are included in the result set
			AND ProfileSource = ''CareerCircle''
			AND ProfileFormat = ''Json''
			AND spss.IsDeleted = 0
		WHERE s.IsDeleted = 0)
	SELECT Source, Referrer, Count(1) [Count]
	FROM subscriberSources
	GROUP BY Source, Referrer
')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
