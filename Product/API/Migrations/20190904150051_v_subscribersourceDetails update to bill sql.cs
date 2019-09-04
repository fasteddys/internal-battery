using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class v_subscribersourceDetailsupdatetobillsql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
 
     

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
            



')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
