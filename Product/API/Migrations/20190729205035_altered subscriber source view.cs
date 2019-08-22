using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class alteredsubscribersourceview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

EXEC('
/*
<remarks>
2019-03-14 - Bill Koenig - Created
2019-04-19 - Jim Brazil - Updated to include partner name and Id
2019-07-29 - Jim Brazil - Do-over to support new partner source data model
</remarks>
<description>
Returns subscriber sources aggregated with counts for number of subscribers
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberSources]
</example>
*/
ALTER VIEW [dbo].[v_SubscriberSources]
AS
SELECT        p.Name, COUNT(*) AS Count, '' '' AS Referrer, p.Name AS Source, p.PartnerId
FROM            dbo.GroupPartner AS gp INNER JOIN
                         dbo.SubscriberGroup AS sg ON gp.GroupId = sg.GroupId INNER JOIN
                         dbo.Partner AS p ON gp.PartnerId = p.PartnerId
WHERE        (p.IsDeleted = 0) AND (sg.IsDeleted = 0) AND (gp.IsDeleted = 0)
GROUP BY p.Name, p.PartnerId')
 
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

EXEC('
/*
<remarks>
2019-03-14 - Bill Koenig - Created
2019-04-19 - Jim Brazil - Updated to include partner name and Id
</remarks>
<description>
Returns subscriber sources aggregated with counts for number of subscribers
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberSources]
</example>
*/
alter VIEW [dbo].[v_SubscriberSources]
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
	SELECT Source, Referrer, Count(1) [Count], p.Name, p.PartnerId
	FROM subscriberSources as s, PartnerReferrer as pr, [Partner] as p
	WHERE Referrer = pr.Path and pr.PartnerId = p.PartnerId
	GROUP BY Source, p.Name, p.PartnerId, Referrer
	UNION ALL
	SELECT ''Any'', ''Any'', COUNT(s.SubscriberId), ''Any'', -1
	FROM Subscriber s WITH(NOLOCK) 
	WHERE s.IsDeleted = 0
')
 
            ");


        }
    }
}
