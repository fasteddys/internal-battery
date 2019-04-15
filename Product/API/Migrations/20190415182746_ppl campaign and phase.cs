using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class pplcampaignandphase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
declare @newCampaign table (CampaignId int)
insert into Campaign (IsDeleted, CreateDate, CreateGuid, CampaignGuid, Name, StartDate, Description)
output inserted.CampaignId into @newCampaign(CampaignId)
values (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', NEWID(), 'PPL Lead Gen', GETUTCDATE(), 'Contacts associated with this campaign were purchased through the pay per lead channel.')

insert into CampaignPhase (IsDeleted, CreateDate, CreateGuid, CampaignPhaseGuid, Name, Description, CampaignId)
select 0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', NEWID(), 'initial', 'First PPL campaign phase', CampaignId
from @newCampaign
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
delete cp
from Campaign c
inner join CampaignPhase cp on c.CampaignId = cp.CampaignId
where c.Name = 'PPL Lead Gen'

delete from campaign where name = 'PPL Lead Gen'
            ");
        }
    }
}
