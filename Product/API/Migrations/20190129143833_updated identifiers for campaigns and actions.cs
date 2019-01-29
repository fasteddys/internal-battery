using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatedidentifiersforcampaignsandactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Campaign");

            migrationBuilder.AlterColumn<Guid>(
                name: "CampaignGuid",
                table: "Campaign",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ActionGuid",
                table: "Action",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CampaignGuid",
                table: "Campaign",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Campaign",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActionGuid",
                table: "Action",
                nullable: true,
                oldClrType: typeof(Guid));
        }
    }
}
