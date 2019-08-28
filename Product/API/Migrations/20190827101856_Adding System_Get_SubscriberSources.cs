using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingSystem_Get_SubscriberSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('

    /*
    <remarks>
    2019.08.22 - Jim Brazil - Created

	<description>
    Returns subscriber source info
    </description>
	  
    */
    CREATE PROCEDURE [dbo].[System_Get_SubscriberSources ] (
        @SubscriberId Int 
    )
    AS
    BEGIN

	SELECT  * from [v_SubscriberSourceDetails] where subscriberId = @SubscriberId    	   
   
    END
 
 
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Get_SubscriberSources]
            ");

        }
    }
}
