using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforTopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TopicName",
                table: "Topic",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TopicGuid",
                table: "Topic",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Topic",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TopicName",
                table: "Topic",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<Guid>(
                name: "TopicGuid",
                table: "Topic",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Topic",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
