using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class alterringTopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Topic",
                newName: "TopicName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Topic",
                newName: "TabletImage");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Topic",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Topic",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Topic",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesktopImage",
                table: "Topic",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileImage",
                table: "Topic",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Topic",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Topic",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Topic",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TopicGuid",
                table: "Topic",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "DesktopImage",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "MobileImage",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "TopicGuid",
                table: "Topic");

            migrationBuilder.RenameColumn(
                name: "TopicName",
                table: "Topic",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TabletImage",
                table: "Topic",
                newName: "Code");
        }
    }
}
