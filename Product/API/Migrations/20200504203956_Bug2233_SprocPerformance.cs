using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Bug2233_SprocPerformance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.03.10 - Bill Koenig - Created
2020.03.29 - Joey Herrington - Added PhoneNumber, City, State, Postal, and Title to the SELECT list
2020.04.14 - Joey Herrington - Added CreatedDate and ModifiedDate to the SELECT list
2020.05.04 - Joey Herrington - Rewrote sproc to address performance degredation
</remarks>
<description>
Returns profile wishlist details. The subscriber guid is a security measure to ensure that wishlists cannot be viewed by people other than the person that created it.
</description>
<example>
EXEC [G2].[System_Get_ProfileWishlistsForRecruiter] @WishlistGuid = ''A07DD85B-01E4-402D-97FD-D6A2504E5425'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [G2].[System_Get_ProfileWishlistsForRecruiter] (
	@WishlistGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN
    WITH allRecords AS (
        SELECT
            pwl.ProfileWishlistGuid,
            p.ProfileGuid,
            r.RecruiterGuid,
            COALESCE(p.Email, s.Email) AS Email,
            COALESCE(p.FirstName, s.FirstName) AS FirstName,
            COALESCE(p.LastName, s.LastName) AS LastName,
            COALESCE(p.PhoneNumber, s.PhoneNumber) AS PhoneNumber,
            COALESCE(pC.[Name], s.City) AS City,
            COALESCE(pS.[Name], sS.[Name]) AS [State],
            COALESCE(pP.[Code], s.PostalCode) AS Postal,
            COALESCE(p.Title, s.Title) AS Title,
            pwl.CreateDate,
            pwl.ModifyDate
        FROM dbo.Subscriber s
            JOIN dbo.Recruiter r ON r.SubscriberId = s.SubscriberId
            JOIN G2.Wishlists wl ON r.RecruiterId = wl.RecruiterId
            JOIN G2.ProfileWishlists pwl ON wl.WishlistId = pwl.WishlistId
            JOIN G2.Profiles p ON pwl.ProfileId = p.ProfileId
            LEFT JOIN dbo.City pC ON p.CityId = pC.CityId
            LEFT JOIN dbo.[State] pS ON p.StateId = pS.StateId
            LEFT JOIN dbo.[State] sS on s.StateId = sS.StateId
            LEFT JOIN dbo.Postal pP ON p.PostalId = pP.PostalId
        WHERE s.IsDeleted = 0 AND r.IsDeleted = 0 AND wl.IsDeleted = 0 AND pwl.IsDeleted = 0 AND p.IsDeleted = 0
            AND s.SubscriberGuid = @SubscriberGuid
            AND wl.WishlistGuid = @WishlistGuid
    )
    SELECT
        ProfileWishlistGuid,
        ProfileGuid,
        RecruiterGuid,
        Email,
        FirstName,
        LastName,
        PhoneNumber,
        City,
        [State],
        Postal,
        Title,
        CreateDate,
        ModifyDate,
        (SELECT COUNT(1) FROM allRecords) AS TotalRecords
    FROM allRecords
    ORDER BY  
        CASE WHEN @Order = 'ascending' AND @Sort = 'firstName' THEN FirstName END,
        CASE WHEN @Order = 'ascending' AND @Sort = 'lastName' THEN LastName END,
        CASE WHEN @Order = 'ascending' AND @Sort = 'createDate' THEN CreateDate END,
        CASE WHEN @Order = 'ascending' AND @Sort = 'modifyDate' THEN ModifyDate END, 
        CASE WHEN @Order = 'ascending' AND @Sort = 'email' THEN Email END, 
        CASE WHEN @Order = 'descending' AND @Sort = 'firstName' THEN FirstName END DESC ,
        CASE WHEN @Order = 'descending' AND @Sort = 'lastName' THEN LastName END DESC ,
        CASE WHEN @Order = 'descending' AND @Sort = 'createDate' THEN CreateDate END DESC ,
        CASE WHEN @Order = 'descending' AND @Sort = 'modifyDate' THEN ModifyDate END DESC ,
        CASE WHEN @Order = 'descending' AND @Sort = 'email' THEN Email END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.03.10 - Bill Koenig - Created
2020.03.29 - Joey Herrington - Added PhoneNumber, City, State, Postal, and Title to the SELECT list
2020.04.14 - Joey Herrington - Added CreatedDate and ModifiedDate to the SELECT list
</remarks>
<description>
Returns profile wishlist details. The subscriber guid is a security measure to ensure that wishlists cannot be viewed by people other than the person that created it.
</description>
<example>
EXEC [G2].[System_Get_ProfileWishlistsForRecruiter] @WishlistGuid = ''A07DD85B-01E4-402D-97FD-D6A2504E5425'', @SubscriberGuid = ''03FFCC3D-CC3A-414E-98E3-2FC5541CB6CB'', @Limit = 10, @Offset = 0, @Sort = ''modifyDate'', @Order = ''descending''
</example> 
*/
ALTER PROCEDURE [G2].[System_Get_ProfileWishlistsForRecruiter] (
	@WishlistGuid UNIQUEIDENTIFIER,
	@SubscriberGuid UNIQUEIDENTIFIER,
    @Limit INT,
    @Offset INT,
    @Sort VARCHAR(MAX),
    @Order VARCHAR(MAX)
)
AS
BEGIN 
    WITH allRecords AS (
    	SELECT pw.ProfileWishlistId
    	FROM G2.ProfileWishlists pw
		INNER JOIN G2.Wishlists w ON pw.WishlistId = w.WishlistId
		INNER JOIN G2.Profiles p ON pw.ProfileId = p.ProfileId	
		INNER JOIN dbo.Recruiter r ON w.RecruiterId = r.RecruiterId
		INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
		INNER JOIN G2.v_ProfileAzureSearch v ON p.ProfileGuid = v.ProfileGuid
    	WHERE w.IsDeleted = 0
		AND pw.IsDeleted = 0
		AND w.WishlistGuid = @WishlistGuid
		AND s.SubscriberGuid = @SubscriberGuid
    )
    SELECT pw.ProfileWishlistGuid
		, p.ProfileGuid
		, r.RecruiterGuid
		, v.Email
		, v.FirstName
		, v.LastName
		, v.PhoneNumber
		, v.City
		, v.[State]
		, v.Postal
		, v.Title
		, pw.CreateDate
		, pw.ModifyDate
    	, (SELECT COUNT(1) FROM allRecords) [TotalRecords]
    FROM G2.ProfileWishlists pw
	INNER JOIN G2.Wishlists w ON pw.WishlistId = w.WishlistId
	INNER JOIN G2.Profiles p ON pw.ProfileId = p.ProfileId	
	INNER JOIN dbo.Recruiter r ON w.RecruiterId = r.RecruiterId
	INNER JOIN Subscriber s ON r.SubscriberId = s.SubscriberId
	INNER JOIN G2.v_ProfileAzureSearch v ON p.ProfileGuid = v.ProfileGuid
    WHERE w.IsDeleted = 0
	AND pw.IsDeleted = 0
	AND w.WishlistGuid = @WishlistGuid
	AND s.SubscriberGuid = @SubscriberGuid
    ORDER BY  
    CASE WHEN @Order = ''ascending'' AND @Sort = ''firstName'' THEN v.FirstName END,
	CASE WHEN @Order = ''ascending'' AND @Sort = ''lastName'' THEN v.LastName END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''createDate'' THEN pw.CreateDate END,
    CASE WHEN @Order = ''ascending'' AND @Sort = ''modifyDate'' THEN pw.ModifyDate END, 
	CASE WHEN @Order = ''ascending'' AND @Sort = ''email'' THEN v.Email END, 
    CASE WHEN @Order = ''descending'' AND @Sort = ''firstName'' THEN v.FirstName END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''lastName'' THEN v.LastName END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''createDate'' THEN pw.CreateDate END DESC ,
    CASE WHEN @Order = ''descending'' AND @Sort = ''modifyDate'' THEN pw.ModifyDate END DESC ,
	CASE WHEN @Order = ''descending'' AND @Sort = ''email'' THEN v.Email END DESC 
    OFFSET @Offset ROWS
    FETCH FIRST @Limit ROWS ONLY
END')");
		}
	}
}
