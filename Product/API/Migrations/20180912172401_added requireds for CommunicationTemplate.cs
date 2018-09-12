using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforCommunicationTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunicationTemplateGuid",
                table: "CommunicationTemplate",
                nullable: true,
                oldClrType: typeof(Guid));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CommunicationTemplateGuid",
                table: "CommunicationTemplate",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
