using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class WozU_Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
UPDATE 
	v 
SET
v.Name = 'Woz U'
FROM Vendor v
WHERE v.VendorGuid = '00000000-0000-0000-0000-000000000001'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
