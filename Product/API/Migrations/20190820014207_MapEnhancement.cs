using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class MapEnhancement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC('
                  CREATE PROCEDURE [dbo].[System_JobCountPerProvince] 
                AS
                BEGIN
                    SELECT jp.[Province]
                        ,c.CompanyName
                        ,c.CompanyGuid
                        ,count(*) AS Count
                    FROM JobPosting jp
                    JOIN Company c ON jp.CompanyId = c.CompanyId
                    WHERE jp.IsDeleted = 0
                    GROUP BY c.CompanyName
                        ,jp.Province
                        ,c.CompanyGuid
                END
                ')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" DROP PROCEDURE [dbo].[System_JobCountPerProvince]");
        }
    }
}
