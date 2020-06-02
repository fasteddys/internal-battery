using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class TEKsystemsjobscrapechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("JobSite", "Name", "TEKsystems", new string[] { "IsDeleted", "ModifyDate", "Uri", "PercentageReductionThreshold", "CrawlDelayInMilliseconds" }, new object[] { false, DateTime.UtcNow, @"https://careers.teksystems.com/us/en/sitemap.xml", null, null }, "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
