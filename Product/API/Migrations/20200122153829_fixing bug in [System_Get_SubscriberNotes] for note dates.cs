using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class fixingbuginSystem_Get_SubscriberNotesfornotedates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
2020.01.16 - Bill Koenig - Added support for total records, fixed sort and order, added example
2020.01.22 - JAB - fixed bug of returning subscriber mod/create date rather that the notes 
</remarks>
<description>
Returns notes for subscriber 
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotes] @SubscriberGuid = ''df7a8931-c99b-40a0-b117-230a203db400'', @TalentGuid = ''47568e38-a8d5-440e-b613-1c0c75787e90'', @Limit = 10, @Offset = 0, @Sort = ''recruiter'', @Order = ''ascending''
</example>
 */
ALTER PROCEDURE [dbo].[System_Get_SubscriberNotes] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @TalentGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT sn.SubscriberNotesId
		FROM SubscriberNotes sn
		INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
		INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
		WHERE rs.SubscriberGuid = @SubscriberGuid 
		AND s.SubscriberGuid = @TalentGuid  
		AND sn.IsDeleted = 0  
	)
    SELECT sn.SubscriberNotesGuid, 
		s.SubscriberGuid,
		r.RecruiterGuid,
		r.FirstName + '' '' + r.LastName RecruiterName,
		sn.Notes,
		sn.ViewableByOthersInRecruiterCompany,
		sn.CreateDate,
		sn.ModifyDate as ModifiedDate,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM SubscriberNotes sn
	INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
	INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
	INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
	WHERE rs.SubscriberGuid = @SubscriberGuid 
	AND s.SubscriberGuid = @TalentGuid   
	AND sn.IsDeleted = 0   
	ORDER BY  
	CASE WHEN @Order = ''ascending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN sn.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN sn.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>
2019.12.02 - Jim Brazil - Created 
2020.01.16 - Bill Koenig - Added support for total records, fixed sort and order, added example
</remarks>
<description>
Returns notes for subscriber 
</description>
<example>
EXEC [dbo].[System_Get_SubscriberNotes] @SubscriberGuid = ''df7a8931-c99b-40a0-b117-230a203db400'', @TalentGuid = ''47568e38-a8d5-440e-b613-1c0c75787e90'', @Limit = 10, @Offset = 0, @Sort = ''recruiter'', @Order = ''ascending''
</example>
 */
ALTER PROCEDURE [dbo].[System_Get_SubscriberNotes] (
	@SubscriberGuid UNIQUEIDENTIFIER,
    @TalentGuid UNIQUEIDENTIFIER,
    @Limit int,
    @Offset int,
    @Sort varchar(max),
    @Order varchar(max)
)
AS
BEGIN 
	WITH allRecords AS (
		SELECT sn.SubscriberNotesId
		FROM SubscriberNotes sn
		INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
		INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
		WHERE rs.SubscriberGuid = @SubscriberGuid 
		AND s.SubscriberGuid = @TalentGuid  
		AND sn.IsDeleted = 0  
	)
    SELECT sn.SubscriberNotesGuid, 
		s.SubscriberGuid,
		r.RecruiterGuid,
		r.FirstName + '' '' + r.LastName RecruiterName,
		sn.Notes,
		sn.ViewableByOthersInRecruiterCompany,
		s.CreateDate,
		s.ModifyDate as ModifiedDate,
		(SELECT COUNT(1) FROM allRecords) [TotalRecords]
	FROM SubscriberNotes sn
	INNER JOIN Subscriber s ON sn.SubscriberId = s.SubscriberId
	INNER JOIN Recruiter r ON sn.RecruiterId = r.RecruiterId
	INNER JOIN Subscriber rs ON r.SubscriberId = rs.SubscriberId
	WHERE rs.SubscriberGuid = @SubscriberGuid 
	AND s.SubscriberGuid = @TalentGuid   
	AND sn.IsDeleted = 0   
	ORDER BY  
	CASE WHEN @Order = ''ascending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN sn.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN sn.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN sn.ModifyDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''recruiter'' THEN r.RecruiterId END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");


        }
    }
}
