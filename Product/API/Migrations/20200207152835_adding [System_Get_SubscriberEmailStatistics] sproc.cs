using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingSystem_Get_SubscriberEmailStatisticssproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('   
/*     
    <remarks>
    2019.02.07 - Jim Brazil - Created 
 
    </remarks>
    <description>
    Returns count and latest date of sendgrid email events for the specified email address 
    </description>
     
    */
    CREATE PROCEDURE [dbo].[System_Get_SubscriberEmailStatistics] 
	(
		@EmailAddress Varchar(Max)    			
    )
    AS    
	BEGIN     

	SELECT 
		* 
	FROM 
	(
		SELECT   
			ROW_NUMBER() OVER (PARTITION BY EVENT ORDER BY CreateDate DESC) as  RowNum, 
			COUNT(*) OVER (PARTITION BY EVENT ORDER BY CreateDate ASC) as NumEvents, 
			[Event],
			Email,
			IsDeleted,
			CreateDate as LatestEventDate,
			[Subject] as LatestEmailSubject
		FROM 
			SubscriberSendgridevent 
		WHERE 
			Email =  @EmailAddress AND 
			IsDeleted = 0
	)  EmailInfo
	WHERE
    RowNum = 1

    END
            ')");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" 
DROP PROCEDURE [dbo].[System_Get_SubscriberEmailStatistics]
            ");

        }
    }
}
