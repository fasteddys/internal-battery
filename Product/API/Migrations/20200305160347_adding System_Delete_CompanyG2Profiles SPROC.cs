using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Delete_CompanyG2ProfilesSPROC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2020.03.03 - JAB - Created

</remarks>
<description>
Deletes all g2 profile records for the given company.  
</description>
<example>
EXEC [dbo].[[System_Delete_CompanyG2Profiles]] @ComopanyGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [dbo].[System_Delete_CompanyG2Profiles] (
	@CompanyGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 

    SELECT * 
	INTO #ProfileIDs 
	FROM
	(
	   SELECT p.ProfileId 
	   FROM G2.Profiles p   	   
	   WHERE p.CompanyId = (SELECT companyid FROM company WHERE companyguid = @CompanyGuid)
	) AS profiles

	--- Delete from Profile Comments 
	UPDATE G2.ProfileComments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Documents 
	UPDATE G2.ProfileDocuments
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Search Locations 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profile Skills 
	Update G2.ProfileSearchLocations
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile Tags 
	Update G2.ProfileTags
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )
	
	--- Delete from Profile WishLists  
	Update G2.ProfileWishlists
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )

	--- Delete from Profiles 
	Update G2.Profiles
	SET IsDeleted = 1
	WHERE ProfileId IN ( SELECT * FROM #ProfileIDs )  

 
END
')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE [dbo].[System_Delete_CompanyG2Profiles]
            ");
        }
    }
}
