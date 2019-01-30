using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removedlinkidentifierfromcontact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkIdentifier",
                table: "Contact");

            migrationBuilder.AlterColumn<Guid>(
                name: "ContactGuid",
                table: "Contact",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ContactGuid",
                table: "Contact",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<string>(
                name: "LinkIdentifier",
                table: "Contact",
                nullable: false,
                defaultValue: "");
        }
    }
}
