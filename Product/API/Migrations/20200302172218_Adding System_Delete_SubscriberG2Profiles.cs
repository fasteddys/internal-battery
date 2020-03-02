using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Delete_SubscriberG2Profiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('   
/*
<remarks>
2020.03.02 - JAB - Created

</remarks>
<description>
Deletes all subcriber g2 profile records for the given subscriber.  
</description>
<example>
EXEC [dbo].[[System_Delete_SubscriberG2Profiles]] @SubscriberGuid = ''89C78E99-6A16-42B1-B4BB-F5F98F6B74A9''
</example>
*/
CREATE PROCEDURE [dbo].[System_Delete_SubscriberG2Profiles] (
	@SubscriberGuid UNIQUEIDENTIFIER 
)
AS
BEGIN 

    SELECT * 
	INTO #ProfileIDs 
	FROM
	(
	   SELECT p.ProfileId 
	   FROM G2.Profiles p 
       JOIN Subscriber s ON p.SubscriberId = s.SubscriberId	   	   
	   WHERE s.SubscriberGuid = @SubscriberGuid
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
DROP PROCEDURE [dbo].[System_Delete_SubscriberG2Profiles]
            ");
        }
    }
}
