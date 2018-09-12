using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedandupdatesNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SubText",
                table: "News",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "NewsGuid",
                table: "News",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<string>(
                name: "Headline",
                table: "News",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLink",
                table: "News",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NewsTypeId",
                table: "News",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalLink",
                table: "News");

            migrationBuilder.DropColumn(
                name: "NewsTypeId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "SubText",
                table: "News",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<Guid>(
                name: "NewsGuid",
                table: "News",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Headline",
                table: "News",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
