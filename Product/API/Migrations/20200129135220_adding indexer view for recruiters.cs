using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingindexerviewforrecruiters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"  
/*
  Drop the view if it exists - this is necessary since the view has to be created in stage
  for development.

 */

IF OBJECT_ID('[v_RecruiterIndexerView]') IS NOT  NULL
  DROP VIEW v_RecruiterIndexerView
 

");



 migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-01-28 - Jim Brazil - Created
</remarks>
<description>
Returns recruiter information for use with Azure Search
</description>
<example>
SELECT * FROM [dbo].[v_RecruiterIndexerView] ORDER BY ModifyDate DESC
</example>
*/
CREATE VIEW [dbo].[v_RecruiterIndexerView]
AS
SELECT r.IsDeleted
	,r.CreateDate
	,r.ModifyDate
	,r.RecruiterGuid
	,r.FirstName
	,r.LastName
	,r.Email
	,r.PhoneNumber
	,s.SubscriberGuid
	,c.CompanyGuid
	,c.CompanyName
FROM recruiter r
LEFT JOIN subscriber s ON s.SubscriberId = r.SubscriberId
LEFT JOIN company c ON c.CompanyId = r.CompanyId
WHERE r.IsDeleted = 0
 
')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('  
   DROP VIEW v_RecruiterIndexerView
')");



        }
    }
}
