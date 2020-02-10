using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Purge_SendGridEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('   
/*
<remarks>
2020-02-06 - Jim Brazil - Created
 
</remarks>
<description>
Performs a physical delete for SendGrid event records which are older than the number of days specified by @LookbackDays
</description>
<example>
 
EXEC [dbo].[System_Purge_SendGridEvents] @LookbackDays=30

</example>
*/
CREATE PROCEDURE [dbo].[System_Purge_SendGridEvents] (
	@LookbackDays  Int 
)
AS
BEGIN
 
 DELETE FROM SendGridEvent WHERE CreateDate <  DATEADD(day, (@LookbackDays * -1), GETDATE() )	 
  
END
            ')");



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Purge_SendGridEvents]
            ");

        }
    }
}
