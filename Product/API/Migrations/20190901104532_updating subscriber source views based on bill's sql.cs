using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatingsubscribersourceviewsbasedonbillssql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('



/*
<remarks>
2019-03-14 - Bill Koenig - Created
2019-04-19 - Jim Brazil - Updated to include partner name and Id
2019-07-29 - Jim Brazil - Do-over to support new partner source data model
2019-09-01 - Jim Brazil - Updated with Bill''s sql
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
 	SELECT PartnerName [Name], COUNT(1) [Count], '' '' [Referrer], PartnerName [Source], PartnerId, PartnerGuid
	FROM [dbo].[v_SubscriberSourceDetails]
	WHERE GroupRank = 1 AND PartnerRank = 1
	GROUP BY PartnerName, PartnerGuid, PartnerId
	UNION
	SELECT ''Any'', (SELECT COUNT(DISTINCT SubscriberId) FROM [v_SubscriberSourceDetails]), ''Any'', ''Any'', -1, NULL
 
 
            ')");



            migrationBuilder.Sql(@"

 

/*
<remarks>
2019-08-23 - Jim Brazil - Created
2019-09-01 - Jim Brazil - Updated with Bill's sql
 
</remarks>
<description>
Returns subscriber source details sorted by their group creation date
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberInitialSource]
</example>
*/

ALTER VIEW [dbo].[v_SubscriberSourceDetails]
AS

	WITH rankedGroup AS (
		SELECT sg.SubscriberId, sg.GroupId, ROW_NUMBER() OVER (PARTITION BY sg.SubscriberId ORDER BY sg.CreateDate ASC) as [GroupRank]
		FROM SubscriberGroup sg
		WHERE IsDeleted = 0
	), rankedGroupPartner AS (
		SELECT rg.SubscriberId, rg.GroupId, rg.GroupRank, gp.PartnerId, ROW_NUMBER() OVER (PARTITION BY rg.SubscriberId ORDER BY gp.CreateDate ASC) as [PartnerRank]
		FROM rankedGroup rg
		INNER JOIN GroupPartner gp ON rg.GroupId = gp.GroupId
		WHERE gp.IsDeleted = 0
	), attribution AS (
		SELECT s.SubscriberId, rgp.GroupId, rgp.GroupRank, rgp.PartnerId, rgp.PartnerRank [PartnerRank]
		FROM rankedGroupPartner rgp
		INNER JOIN Subscriber s ON rgp.SubscriberId = s.SubscriberId
	)
	SELECT 
	-- subscriber properties
	s.SubscriberId, s.SubscriberGuid, s.Email, s.FirstName, s.LastName
	-- group properties
	, a.GroupRank, g.GroupGuid, g.[Name] [GroupName]
	-- partner properties
	, a.PartnerRank, p.PartnerGuid, p.[Name] [PartnerName], p.PartnerId
	FROM Subscriber s
	LEFT JOIN attribution a ON s.SubscriberId = a.SubscriberId
	LEFT JOIN [Group] g ON a.GroupId = g.GroupId
	LEFT JOIN [Partner] p ON a.PartnerId = p.PartnerId
	WHERE s.IsDeleted = 0
            
 
            ");



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('


/*
<remarks>
2019-08-23 - Jim Brazil - Created
 
</remarks>
<description>
Returns subscriber source details sorted by their group creation date
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberInitialSource]
</example>
*/

ALTER VIEW [dbo].[v_SubscriberSourceDetails]
AS

SELECT ROW_NUMBER()   OVER (Partition by s.subscriberId order by sg.CreateDate asc) as [Rank],  
	  s.SubscriberId,
	  s.SubscriberGuid, 
	  s.Email, 
	  s.FirstName, 
	  s.LastName, 
	  sg.CreateDate as GroupCreateDate,  
	  p.Name as PartnerName,
	  p.PartnerGuid,
	  g.Name as GroupName,
	  g.GroupGuid


FROM
subscriber s
left join SubscriberGroup sg on  s.SubscriberId = sg.SubscriberId
left join [group] g on g.GroupId = sg.GroupId
left join GroupPartner gp on gp.GroupId = sg.GroupId
join Partner p on p.PartnerId = gp.PartnerId
 
            ')");



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
    }
}
