using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class WishListProfile_Addaddldata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.03.10 - Bill Koenig - Created
2020.03.29 - Joey Herrington - Added PhoneNumber, City, State, Postal, and Title to the SELECT list
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*     
<remarks>     
2020.03.10 - Bill Koenig - Created
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
