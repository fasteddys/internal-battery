using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforEducationLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EducationLevelGuid",
                table: "EducationLevel",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EducationLevelGuid",
                table: "EducationLevel",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
