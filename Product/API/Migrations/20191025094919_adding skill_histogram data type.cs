using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingskill_histogramdatatype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC('
/*
<remarks>
2019.03.14 - Bill Koenig - Created    
</remarks>
<description>
A simple user-defined table type that supports a skill histogram.
</description>
*/
CREATE TYPE [dbo].[SkillHistogram] AS TABLE(
	[Skill] [nvarchar](450) NOT NULL,
	[Occurrences] [int] NOT NULL
)
')      
");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TYPE [dbo].[SkillHistogram]
            ");

        }
    }
}
