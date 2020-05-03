using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class recruitercompanyrelationshipupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recruiter_Company_CompanyId",
                table: "Recruiter");

            migrationBuilder.DropIndex(
                name: "IX_Recruiter_CompanyId",
                table: "Recruiter");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Recruiter");

            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.01.22 - JAB - Created
2020.01.24 - Bill Koenig - Corrected description in comment block and fixed name of column ''IsInAuth0RecruiterGroup''
2020.01.29 - JAB - Added support for CompanyName
2020.03.19 - Bill Koenig - Changed recruiter : company association to use the 1 : many, but return only the most recently created 
	company for now. Limited to only internal recruiters (ones with a subscriber record). External recruiters are managed by the
	job data mining process and should not be edited or deleted by admins.
</remarks>
<description>
Returns recruiters
</description>
<example>
EXEC [dbo].[System_Get_Recruiters] @Limit = 1000, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [dbo].[System_Get_Recruiters] (
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT RecruiterId
    	FROM Recruiter r
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
    	WHERE r.IsDeleted = 0 AND s.IsDeleted = 0
    ), recruiterCompanyWithRowNum AS (
		SELECT RecruiterId, CompanyId, ROW_NUMBER() OVER (PARTITION BY RecruiterId ORDER BY CreateDate DESC) rownum
		FROM RecruiterCompany 		
	), mostRecentRecruiterCompany AS (
		SELECT RecruiterId, CompanyId
		FROM recruiterCompanyWithRowNum
		WHERE rownum = 1)
    SELECT  
		r.RecruiterGuid,
		s.SubscriberGuid,
		c.CompanyGuid,
	    c.CompanyName,
		r.FirstName,
		r.LastName,
		r.Email,
		r.PhoneNumber,
		r.CreateDate,
		r.ModifyDate,
		NULL IsInAuth0RecruiterGroup, 
    	(SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM Recruiter r 
	INNER JOIN Subscriber s on r.SubscriberId = s.SubscriberId
	LEFT JOIN mostRecentRecruiterCompany rc ON r.RecruiterId = rc.RecruiterId
	LEFT JOIN Company c on c.CompanyId = rc.CompanyId
    WHERE r.IsDeleted = 0 AND s.IsDeleted = 0
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''firstName'' THEN r.FirstName END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''lastName'' THEN r.LastName END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN r.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN r.ModifyDate END, 
	CASE WHEN @Order = ''ascending'' AND @Sort = ''email'' THEN r.Email END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''firstName'' THEN r.FirstName END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''lastName'' THEN r.LastName END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN r.CreateDate END DESC,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN r.ModifyDate END DESC,
	CASE WHEN @Order = ''descending'' AND @Sort = ''email'' THEN r.Email END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2019-01-28 - Jim Brazil - Created
2020.03.19 - Bill Koenig - Changed recruiter : company association to use the 1 : many, but return only the most recently created 
	company for now. Limited to only internal recruiters (ones with a subscriber record). External recruiters are managed by the
	job data mining process and should not be edited or deleted by admins.
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
		,r.FirstName
		,r.LastName
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
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Recruiter",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recruiter_CompanyId",
                table: "Recruiter",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recruiter_Company_CompanyId",
                table: "Recruiter",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
