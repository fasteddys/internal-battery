using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifyproficiencylevels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.ProficiencyLevels SET ProficiencyLevelName = 'Fluent', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE ProficiencyLevelGuid = '3BAD7FF3-B450-4837-A8A7-3E48FB08DD54'");
            migrationBuilder.Sql("UPDATE dbo.ProficiencyLevels SET ProficiencyLevelName = 'Proficient (moderate)', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE ProficiencyLevelGuid = 'AD93BA96-186C-48B8-9159-59E9FDDA1CD1'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.ProficiencyLevels SET ProficiencyLevelName = 'Limited Working Proficiency', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE ProficiencyLevelGuid = '3BAD7FF3-B450-4837-A8A7-3E48FB08DD54'");
            migrationBuilder.Sql("UPDATE dbo.ProficiencyLevels SET ProficiencyLevelName = 'Bilingual Proficiency', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE ProficiencyLevelGuid = 'AD93BA96-186C-48B8-9159-59E9FDDA1CD1'");
        }
    }
}
