using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class makeJobDataMiningthresholddatadriven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentageReductionThreshold",
                table: "JobSite",
                type: "decimal(3,2)",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE dbo.JobSite SET PercentageReductionThreshold = 0.25, ModifyDate = GETUTCDATE() WHERE [Name] = 'TEKsystems'");
            migrationBuilder.Sql(@"UPDATE dbo.JobSite SET PercentageReductionThreshold = 0.50, ModifyDate = GETUTCDATE() WHERE [Name] = 'Aerotek'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentageReductionThreshold",
                table: "JobSite");
        }
    }
}
