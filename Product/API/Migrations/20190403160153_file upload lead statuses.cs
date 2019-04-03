using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class fileuploadleadstatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                table: "LeadStatus",
                columns: new[] { "LeadStatusId", "LeadStatusGuid", "Name", "Description", "Severity", "IsDeleted", "CreateDate", "CreateGuid" },
                values: new object[,]
                {
                    { 6, new Guid("E2A33DF2-9EBA-481D-989E-A55554E7C602"), "Updated", "Lead has been processed successfully and updated", "Information", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") }
                 });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
