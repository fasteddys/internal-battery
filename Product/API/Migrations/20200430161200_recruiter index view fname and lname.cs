using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class recruiterindexviewfnameandlname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019-01-28 - Jim Brazil - Created
2020.03.19 - Bill Koenig - Changed recruiter : company association to use the 1 : many, but return only the most recently created 
	company for now. Limited to only internal recruiters (ones with a subscriber record). External recruiters are managed by the
	job data mining process and should not be edited or deleted by admins.
2020.04.30 - Bill Koenig - Using the first and last name from the Subscriber table (if a value exists)
</remarks>
<description>
Returns recruiter information for use with Azure Search
</description>
<example>
SELECT * FROM [dbo].[v_RecruiterIndexerView] ORDER BY ModifyDate DESC
</example>
*/
ALTER VIEW [dbo].[v_RecruiterIndexerView]
AS

	WITH recruiterCompanyWithRowNum AS (
		SELECT RecruiterId, CompanyId, ROW_NUMBER() OVER (PARTITION BY RecruiterId ORDER BY CreateDate DESC) rownum
		FROM RecruiterCompany 		
	), mostRecentRecruiterCompany AS (
		SELECT RecruiterId, CompanyId
		FROM recruiterCompanyWithRowNum
		WHERE rownum = 1)
	SELECT r.IsDeleted
		,r.CreateDate
		,r.ModifyDate
		,r.RecruiterGuid
		,COALESCE(s.FirstName, r.FirstName) FirstName
		,COALESCE(s.LastName, r.LastName) LastName
		,r.Email
		,r.PhoneNumber
		,s.SubscriberGuid
		,c.CompanyGuid
		,c.CompanyName
	FROM Recruiter r 
		INNER JOIN Subscriber s on r.SubscriberId = s.SubscriberId
		LEFT JOIN mostRecentRecruiterCompany rc ON r.RecruiterId = rc.RecruiterId
		LEFT JOIN Company c on c.CompanyId = rc.CompanyId
		WHERE r.IsDeleted = 0 AND s.IsDeleted = 0')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
