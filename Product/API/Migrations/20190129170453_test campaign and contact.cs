using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class testcampaignandcontact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            /* FirstName and LastName no longer exist in the dbo.Contact table - this is preventing migrations from running, so omitting this from future sql migrations
            migrationBuilder.InsertData(
                table: "Campaign",
                columns: new[] { "IsDeleted", "CampaignGuid", "CreateDate", "CreateGuid", "Name", "Description", "StartDate" },
                values: new object[] { 0, Guid.Parse("E86F9017-C497-40D6-A75B-3DB73D18266E"), DateTime.UtcNow, Guid.Empty, "Test Campaign Name", "Test Campaign Description", DateTime.UtcNow }
                );

            migrationBuilder.InsertData(
                table: "Contact",
                columns: new[] { "IsDeleted", "ContactGuid", "CreateDate", "CreateGuid", "Email", "FirstName", "LastName" },
                values: new object[] { 0, Guid.Parse("E86F49B0-D981-4B2D-A28D-458D94CA51D3"), DateTime.UtcNow, Guid.Empty, "bobsmith@comcast.net", "Bob", "Smith" }
                );
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
