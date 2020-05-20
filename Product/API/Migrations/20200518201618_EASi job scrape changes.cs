using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class EASijobscrapechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("JobSite", "Name", "EASi", new string[] { "IsDeleted", "ModifyDate", "Uri", "PercentageReductionThreshold", "CrawlDelayInMilliseconds" }, new object[] { false, DateTime.UtcNow, @"https://jobs.easi.com/sitemap.xml", null, null }, "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
