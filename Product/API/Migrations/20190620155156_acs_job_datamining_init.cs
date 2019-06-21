using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class acs_job_datamining_init : Migration
    {
        public static Guid ACS_GUID = new Guid("DF3C6CBF-7A9C-4491-93F2-7DF92AC8BF2E");
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JobSite",
                columns: new[] { "JobSiteGuid", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Uri" },
                values: new object[] { acs_job_datamining_init.ACS_GUID,
                        new DateTime(2019, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified),
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        1,
                        "Allegis Group ICIMS",
                        "https://careers-allegisgroup.icims.com/jobs/search?in_iframe=1"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("JobSite", "JobSiteGuid", acs_job_datamining_init.ACS_GUID);
        }
    }
}
