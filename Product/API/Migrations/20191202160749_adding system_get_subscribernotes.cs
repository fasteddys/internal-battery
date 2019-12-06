using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingsystem_get_subscribernotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
 
</remarks>
<description>
Returns notes for subscriber 
</description>
 
*/


CREATE PROCEDURE [dbo].[System_Get_SubscriberNotes] (
		    @SubscriberGuid UNIQUEIDENTIFIER,
            @TalentGuid UNIQUEIDENTIFIER,
            @Limit int,
            @Offset int,
            @Sort varchar(max),
            @Order varchar(max)
        )
        AS
        BEGIN 
          	SELECT 
			sn.SubscriberNotesGuid, 
			s.SubscriberGuid,
			r.RecruiterGuid,
			r.FirstName + '' '' + r.LastName as RecruiterName,
			sn.Notes,
			sn.ViewableByOthersInRecruiterCompany,
			s.CreateDate,
			s.ModifyDate as ModifiedDate 
			FROM 
			SubscriberNotes sn
			LEFT JOIN Subscriber s on sn.SubscriberId = s.SubscriberId
			LEFT JOIN Recruiter r on sn.RecruiterId = r.RecruiterId
			LEFT JOIN Subscriber rs on r.SubscriberId = rs.SubscriberId
			WHERE
			   s.SubscriberGuid = @TalentGuid and rs.SubscriberGuid = @SubscriberGuid   and sn.IsDeleted = 0   
		    ORDER BY  
			CASE WHEN @Sort = ''ascending'' AND @Order = ''recruiter'' THEN r.RecruiterId END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''createDate'' THEN sn.CreateDate END,
            CASE WHEN @Sort = ''ascending'' AND @Order = ''modifyDate'' THEN sn.ModifyDate END, 
            CASE WHEN @Sort = ''descending'' AND @Order = ''createDate'' THEN sn.CreateDate END desc ,
            CASE WHEN @Sort = ''descending'' AND @Order = ''modifyDate'' THEN sn.ModifyDate END desc ,
			CASE WHEN @Sort = ''descending'' AND @Order = ''recruiter'' THEN r.RecruiterId END desc 
            OFFSET @Offset ROWS
            FETCH FIRST @Limit ROWS ONLY
       END    
')
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Get_SubscriberNotes]
            ");
        }
    }
}
