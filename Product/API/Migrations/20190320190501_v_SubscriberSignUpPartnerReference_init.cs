using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class v_SubscriberSignUpPartnerReference_init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('
                Create View [dbo].[v_SubscriberSignUpPartnerReferences] AS 
	                (SELECT 
		                s.SubscriberId
		                ,p.PartnerId
	                FROM Subscriber s WITH(NOLOCK)
	                LEFT JOIN SubscriberProfileStagingStore spss WITH(NOLOCK) ON s.SubscriberId = spss.SubscriberId
		                AND ProfileSource = ''CareerCircle''
		                AND ProfileFormat = ''Json''
		                AND spss.IsDeleted = 0
	                LEFT JOIN PartnerReferrer pr on JSON_VALUE(spss.ProfileData, ''$.referer'') = pr.Path
	                LEFT JOIN Partner p on pr.PartnerId = p.PartnerId);
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop view [dbo].[v_SubscriberSignUpPartnerReferences]");
        }
    }
}
