using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforCommunicationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunciationTypeGuid",
                table: "CommunicationType",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunciationTypeGuid",
                table: "CommunicationType",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
