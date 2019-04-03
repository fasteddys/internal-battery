using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedv_RecruiterSubscriberActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-03-28 - Bill Koenig - Created
</remarks>
<description>
Returns every tracked subscriber action that is related to a subscriber entity (e.g. viewing a profile or downloading a resume)
</description>
<example>
SELECT * FROM [dbo].[v_RecruiterSubscriberActions]
</example>
*/
CREATE VIEW [dbo].[v_RecruiterSubscriberActions]
AS

	SELECT sa.SubscriberActionGuid, sa.OccurredDate, s.SubscriberGuid [RecruiterGuid], s.Email [RecruiterEmail], s.FirstName [RecruiterFirstName], s.LastName [RecruiterLastName], a.Name [Action], s_et.SubscriberGuid, s_et.Email [SubscriberEmail], s_et.FirstName [SubscriberFirstName], s_et.LastName [SubscriberLastName]
	FROM SubscriberAction sa
	INNER JOIN Subscriber s ON sa.SubscriberId = s.SubscriberId
	INNER JOIN Action a ON sa.ActionId = a.ActionId
	INNER JOIN EntityType et ON sa.EntityTypeId = et.EntityTypeId
	INNER JOIN Subscriber s_et ON sa.EntityId = s_et.SubscriberId AND et.Name = ''Subscriber''

')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW dbo.v_RecruiterSubscriberActions");
        }
    }
}
