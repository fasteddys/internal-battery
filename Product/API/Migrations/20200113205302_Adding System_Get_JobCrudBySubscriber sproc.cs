using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Get_JobCrudBySubscribersproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2020.01.13 - Jim Brazil
 
</remarks>
<description>
Retrieves list of job crud for a subscriber with pagination
</description>
<example>
EXEC [dbo].[System_Get_JobCrudBySubscriber] @SubscriberGuid = ''51605ec8-b62d-4f10-afcb-7edc947ea378'',  @Limit = 10, @Offset = 0, @Order = ''createDate'', @Sort = ''ascending''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_JobCrudBySubscriber] (
    @SubscrberGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
        SELECT JobPostingId
        FROM JobPosting where IsDeleted = 0
    )
    SELECT  
	  jp.*,
	  ISNULL(r.RecruiterGuid,''00000000-0000-0000-0000-000000000000'') as RecruiterGuid,
	  ISNULL(c.CompanyGuid,''00000000-0000-0000-0000-000000000000'') as CompanyGuid,
	  ISNULL(i.IndustryGuid,''00000000-0000-0000-0000-000000000000'') as IndustryGuid,
	  ISNULL(jc.JobCategoryGuid,''00000000-0000-0000-0000-000000000000'') as JobCategoryGuid,
	  ISNULL(el.ExperienceLevelGuid,''00000000-0000-0000-0000-000000000000'') as ExperienceLevelGuid,
	  ISNULL(edl.EducationLevelGuid,''00000000-0000-0000-0000-000000000000'') as EducationLevelGuid,
	  ISNULL(ct.CompensationTypeGuid,''00000000-0000-0000-0000-000000000000'') as CompensationTypeGuid,
	  ISNULL(sc.SecurityClearanceGuid,''00000000-0000-0000-0000-000000000000'') as SecurityClearanceGuid,
	  ISNULL(et.EmploymentTypeGuid,''00000000-0000-0000-0000-000000000000'') as EmploymentTypeGuid ,
	   (SELECT COUNT(1) FROM allRecords) [TotalRecords]

    FROM JobPosting jp    
    LEFT JOIN Recruiter r on  r.RecruiterId = jp.RecruiterId
    LEFT JOIN Subscriber s on r.SubscriberId = s.SubscriberId
	LEFT JOIN Company c on jp.CompanyId = c.CompanyId
	LEFT JOIN Industry i on jp.IndustryId = i.IndustryId
	LEFT JOIN JobCategory jc on jp.JobCategoryId = jc.JobCategoryId
	LEFT JOIN ExperienceLevel el on jp.ExperienceLevelId = el.ExperienceLevelId
	LEFT JOIN EducationLevel edl on jp.ExperienceLevelId = edl.EducationLevelId
	LEFT JOIN CompensationType ct on jp.CompensationTypeId = ct.CompensationTypeId
	LEFT JOIN SecurityClearance sc on jp.SecurityClearanceId = sc.SecurityClearanceId
	LEFT JOIN EmploymentType et on jp.EmploymentTypeId = et.EmploymentTypeId



	WHERE
	    jp.IsDeleted = 0 and s.SubscriberGuid = @SubscrberGuid
    ORDER BY 
    CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN jp.CreateDate END,
    CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN jp.ModifyDate END,
    CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN jp.CreateDate END desc ,
    CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN jp.ModifyDate END desc 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END 
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Get_JobCrudBySubscriber]
            ");

        }
    }
}
