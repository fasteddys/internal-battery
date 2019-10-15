using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class AddingnewPartnerType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                 table: "PartnerType",
                                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "PartnerTypeGuid","Name","Description" },
                                values: new object[,]
                {
                    { 0,
                      new DateTime(2019, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified),
                      new DateTime(2019, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified),
                      Guid.NewGuid(),
                      Guid.NewGuid(),
                      Guid.NewGuid(),
                      "ExternalSource",
                      "External sources that are specified by Source= query string parameter"
                    },
 
                });

        }


 


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
