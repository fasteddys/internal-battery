using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddingAzureIndexStatusTableType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2020.03.16 - JAB - Created    
</remarks>
<description>
A simple user-defined table type that supports pass azure index statues.
</description>
*/
CREATE TYPE [dbo].[AzureIndexStatus] AS TABLE(
	[ErrorMessage] nvarchar(max),
	[ProfileGuid] [UniqueIdentifier] NOT NULL,
	[IndexStatus] [int] NOT NULL
)
')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TYPE [dbo].[SkillHistogram]
            ");

        }
    }
}
