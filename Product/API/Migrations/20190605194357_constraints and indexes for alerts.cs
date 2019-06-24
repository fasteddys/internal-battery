using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class constraintsandindexesforalerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-06-05 - Bill Koenig - Created
</remarks>
<description>
Returns the number of active job posting alerts for a subscriber.
</description>
<example>
SELECT [dbo].[fn_JobPostingAlertsForSubscriber](1084)
</example>
*/
CREATE FUNCTION [dbo].[fn_JobPostingAlertsForSubscriber](@subscriberId INT)
RETURNS TINYINT
AS
BEGIN
	DECLARE @jobPostingAlertsForSubscriberCount TINYINT;
	
	SET @jobPostingAlertsForSubscriberCount = (
		SELECT COUNT(1)
		FROM dbo.JobPostingAlert 
		WHERE SubscriberId = @subscriberId
		AND IsDeleted = 0);

	RETURN @jobPostingAlertsForSubscriberCount;
END
            ')");

            migrationBuilder.Sql(@"EXEC('
ALTER TABLE [dbo].[JobPostingAlert]  WITH CHECK ADD  CONSTRAINT [CK_JobPostingAlert_Max5AlertsPerSubscriber] CHECK  ([dbo].[fn_JobPostingAlertsForSubscriber](SubscriberId) <= 5)
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
DROP FUNCTION [dbo].[fn_JobPostingAlertsForSubscriber]
            ')");

            migrationBuilder.Sql(@"EXEC('
ALTER TABLE [dbo].[JobPostingAlert]  DROP CONSTRAINT [CK_JobPostingAlert_Max5AlertsPerSubscriber]
            ')");
        }
    }
}
