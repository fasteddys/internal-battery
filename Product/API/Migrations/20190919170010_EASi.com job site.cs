using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class EASicomjobsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JobSite",
                columns: new[] { "JobSiteGuid", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Uri", "PercentageReductionThreshold", "CrawlDelayInMilliseconds" },
                values: new object[] { new Guid("B4507F93-72E7-4717-83CE-EB7C349BE8F3"), new DateTime(2019, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "EASi", "https://www.easi.com/jobs/en-US/search", 0.5, 500 });

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "CompanyGuid", "CreateDate", "CreateGuid", "IsDeleted", "CompanyName", "CloudTalentIndexStatus", "IsHiringAgency", "IsJobPoster", "LogoUrl" },
                values: new object[] { new Guid("E7984C36-FD1C-4578-9E66-59305470EF16"), new DateTime(2019, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "EASi", 0, 1, 1, "E7984C36-FD1C-4578-9E66-59305470EF16/easi.png" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JobSite",
                keyColumn: "Name",
                keyValue: "EASi");
        }
    }
}
