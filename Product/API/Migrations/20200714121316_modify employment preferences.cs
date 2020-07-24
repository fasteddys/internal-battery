using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifyemploymentpreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.EmploymentType SET [Name] = 'Remote Preferred', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE EmploymentTypeGuid = '5B0650D7-13F5-4F66-B8B4-18CA8ECE77D2'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.EmploymentType SET [Name] = 'Remote Only', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE EmploymentTypeGuid = '5B0650D7-13F5-4F66-B8B4-18CA8ECE77D2'");
        }
    }
}
