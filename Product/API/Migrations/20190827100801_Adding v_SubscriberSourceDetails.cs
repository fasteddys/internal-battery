using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addingv_SubscriberSourceDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('


/*
<remarks>
2019-08-23 - Jim Brazil - Created
2019-08-27 - Modified to return rownumer as an int 
 
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

SELECT convert(int,ROW_NUMBER()   OVER (Partition by s.subscriberId order by sg.CreateDate asc) ) as [Rank] ,  
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

CREATE VIEW [dbo].[v_SubscriberSourceDetails]
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

        }
    }
}
