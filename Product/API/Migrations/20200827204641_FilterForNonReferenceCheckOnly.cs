using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class FilterForNonReferenceCheckOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
	2020.08.26 - Joey Herrington - Created
	2020.08.27 - Joey Herrington - Added filter to hide candidates who already started a crossChq request
</remarks>
<description>
	Get''s a list of CrossChq statuses by resume upload date
</description>
<example>
	EXEC dbo.System_Get_CrossChqByResumeUploadDate
		@startDate = ''2018-01-01'',
		@showOnlyNonCrossChq = 0,
		@limit = 100,
		@offset = 0,
		@sort = ''CrossChqPercentage'',
		@order = ''descending'';
</example>
*/
ALTER PROCEDURE [dbo].[System_Get_CrossChqByResumeUploadDate] (
	@startDate DATETIME,
	@showOnlyNonCrossChq BIT,
	@limit INT,
	@offset INT,
	@sort NVARCHAR(100),
	@order NVARCHAR(100)
)
AS
BEGIN
	WITH CandidateReferenceCheck AS (
		SELECT
			rc.ReferenceCheckGuid,
			rc.ReferenceCheckType,
			rc.CandidateJobTitle,
			rc.ProfileId,
			rc.RecruiterId,
			rcs.CreateDate StatusDate,
			rcs.Progress,
			rcs.[Status]
		FROM G2.ReferenceCheck rc
			JOIN G2.ReferenceCheckStatus rcs ON rc.ReferenceCheckId = rcs.ReferenceCheckId
			JOIN (
				SELECT _rcs.ReferenceCheckId, MAX(_rcs.CreateDate) CreateDate
				FROM G2.ReferenceCheckStatus _rcs
				WHERE _rcs.isDeleted = 0
				GROUP BY _rcs.ReferenceCheckId
			) mostRecent ON rcs.ReferenceCheckId = mostRecent.ReferenceCheckId AND rcs.CreateDate = mostRecent.CreateDate
		WHERE rc.IsDeleted = 0
	), CandidateResumeStatuses AS (
		SELECT
			s.SubscriberGuid,
			p.ProfileGuid,
			COALESCE(s.FirstName, p.FirstName) FirstName,
			COALESCE(s.LastName, p.LastName) LastName,
			COALESCE(s.Email, p.Email) Email,
			COALESCE(s.PhoneNumber, p.PhoneNumber) PhoneNumber,
			s.CreateDate SubscriberCreateDate,
			sf.CreateDate ResumeUploadedDate,
			DATEDIFF(HOUR, s.CreateDate, sf.CreateDate) ElapsedHoursToUploadResume,
			crc.ReferenceCheckType CrossChqReferenceCheckType,
			crc.CandidateJobTitle CrossChqJobTitle,
			crc.StatusDate CrossChqStatusDate,
			crc.Progress CrossChqPercentage,
			crc.[Status] CrossChqStatus
		FROM dbo.Subscriber s
			JOIN G2.Profiles p ON s.SubscriberId = p.SubscriberId
			LEFT JOIN dbo.SubscriberFile sf ON s.SubscriberId = sf.SubscriberId and sf.IsDeleted =0
			LEFT JOIN CandidateReferenceCheck crc ON p.ProfileId = crc.ProfileId
		WHERE
			p.IsDeleted = 0
			AND (@showOnlyNonCrossChq = 0 OR crc.ReferenceCheckType IS NULL)
			AND s.IsDeleted = 0
			AND s.CreateDate > @startDate
	)
	SELECT
		SubscriberGuid,
		ProfileGuid,
		FirstName,
		LastName,
		Email,
		PhoneNumber,
		SubscriberCreateDate,
		ResumeUploadedDate,
		ElapsedHoursToUploadResume,
		CrossChqReferenceCheckType,
		CrossChqJobTitle,
		CrossChqStatusDate,
		CrossChqPercentage,
		CrossChqStatus,
		(SELECT COUNT(1) FROM CandidateResumeStatuses) TotalRecords
	FROM CandidateResumeStatuses
	ORDER BY
		CASE WHEN @order = ''ascending'' AND @sort = ''FirstName'' THEN FirstName END,
		CASE WHEN @order = ''ascending'' AND @sort = ''LastName'' THEN LastName END,
		CASE WHEN @order = ''ascending'' AND @sort = ''Email'' THEN Email END,
		CASE WHEN @order = ''ascending'' AND @sort = ''PhoneNumber'' THEN PhoneNumber END,
		CASE WHEN @order = ''ascending'' AND @sort = ''SubscriberCreateDate'' THEN SubscriberCreateDate END,
		CASE WHEN @order = ''ascending'' AND @sort = ''ResumeUploadedDate'' THEN ResumeUploadedDate END,
		CASE WHEN @order = ''ascending'' AND @sort = ''ElapsedHoursToUploadResume'' THEN ElapsedHoursToUploadResume END,
		CASE WHEN @order = ''ascending'' AND @sort = ''CrossChqReferenceCheckType'' THEN CrossChqReferenceCheckType END,
		CASE WHEN @order = ''ascending'' AND @sort = ''CrossChqJobTitle'' THEN CrossChqJobTitle END,
		CASE WHEN @order = ''ascending'' AND @sort = ''CrossChqStatusDate'' THEN CrossChqStatusDate END,
		CASE WHEN @order = ''ascending'' AND @sort = ''CrossChqPercentage'' THEN CrossChqPercentage END,
		CASE WHEN @order = ''ascending'' AND @sort = ''CrossChqStatus'' THEN CrossChqStatus END,
		CASE WHEN @order = ''descending'' AND @sort = ''FirstName'' THEN FirstName END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''LastName'' THEN LastName END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''Email'' THEN Email END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''PhoneNumber'' THEN PhoneNumber END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''SubscriberCreateDate'' THEN SubscriberCreateDate END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''ResumeUploadedDate'' THEN ResumeUploadedDate END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''ElapsedHoursToUploadResume'' THEN ElapsedHoursToUploadResume END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''CrossChqReferenceCheckType'' THEN CrossChqReferenceCheckType END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''CrossChqJobTitle'' THEN CrossChqJobTitle END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''CrossChqStatusDate'' THEN CrossChqStatusDate END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''CrossChqPercentage'' THEN CrossChqPercentage END DESC,
		CASE WHEN @order = ''descending'' AND @sort = ''CrossChqStatus'' THEN CrossChqStatus END DESC
	OFFSET @offset ROWS
	FETCH FIRST @limit ROWS ONLY;
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nope!
        }
    }
}
