using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class reportingforofferclaims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-03-28 - Bill Koenig - Created
</remarks>
<description>
Returns every tracked subscriber action that is related to an offer entity (e.g. claiming an offer)
</description>
<example>
SELECT * FROM [dbo].[v_SubscriberOfferActions]
</example>
*/
CREATE VIEW [dbo].[v_SubscriberOfferActions]
AS

	SELECT sa.SubscriberActionGuid, sa.OccurredDate, s.SubscriberGuid [SubscriberGuid], s.Email [SubscriberEmail], s.FirstName [SubscriberFirstName], s.LastName [SubscriberLastName], a.Name [Action], o_et.OfferGuid, o_et.Name [OfferName], o_et.Code [OfferCode]
	FROM SubscriberAction sa
	INNER JOIN Subscriber s ON sa.SubscriberId = s.SubscriberId
	INNER JOIN Action a ON sa.ActionId = a.ActionId
	INNER JOIN EntityType et ON sa.EntityTypeId = et.EntityTypeId
	INNER JOIN Offer o_et ON sa.EntityId = o_et.OfferId AND et.Name = ''Offer''
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW dbo.v_SubscriberOfferActions");
        }
    }
}
