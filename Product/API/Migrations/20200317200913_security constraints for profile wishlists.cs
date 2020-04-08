using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class securityconstraintsforprofilewishlists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-03-17 - Bill Koenig - Created
</remarks>
<description>
Returns TRUE if the recruiter is in the same company as the profile. Otherwise, returns FALSE.
</description>
<example>
SELECT [dbo].[fn_IsProfileInSameCompanyAsWishlistRecruiter](1, 502) --returns FALSE
SELECT [dbo].[fn_IsProfileInSameCompanyAsWishlistRecruiter](1, 379) --returns TRUE
</example>
*/
CREATE FUNCTION [dbo].[fn_IsProfileInSameCompanyAsWishlistRecruiter](@WishlistId INT, @ProfileId INT)
RETURNS BIT
AS
BEGIN

	IF EXISTS (
		SELECT rc.RecruiterCompanyId
		FROM G2.Profiles p
		INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId
		INNER JOIN dbo.RecruiterCompany rc ON c.CompanyId = rc.CompanyId
		INNER JOIN dbo.Recruiter r ON rc.RecruiterId = r.RecruiterId
		INNER JOIN G2.Wishlists w ON r.RecruiterId = w.RecruiterId
		WHERE p.ProfileId = @ProfileId AND w.WishlistId = @WishlistId
		AND rc.IsDeleted = 0)
	BEGIN
		RETURN 1;
	END

	RETURN 0;
END')");

            migrationBuilder.Sql("ALTER TABLE [G2].[ProfileWishlists]  WITH CHECK ADD  CONSTRAINT [CK_ProfileWishlists_IsProfileInSameCompanyAsWishlistRecruiter] CHECK  (([dbo].[fn_IsProfileInSameCompanyAsWishlistRecruiter]([WishlistId],[ProfileId])=(1)))");

            migrationBuilder.Sql("ALTER TABLE [G2].[ProfileWishlists] CHECK CONSTRAINT [CK_ProfileWishlists_IsProfileInSameCompanyAsWishlistRecruiter]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE[G2].[ProfileWishlists] DROP CONSTRAINT [CK_ProfileWishlists_IsProfileInSameCompanyAsWishlistRecruiter]");

            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_IsProfileInSameCompanyAsWishlistRecruiter]");
        }
    }
}
