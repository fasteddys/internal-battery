using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class Aerotekjobscrapechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("JobSite", "Name", "Aerotek", new string[] { "IsDeleted", "ModifyDate", "Uri", "PercentageReductionThreshold", "CrawlDelayInMilliseconds" }, new object[] { false, DateTime.UtcNow, @"https://jobs.aerotek.com/us/en/sitemap_index.xml", null, null }, "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
