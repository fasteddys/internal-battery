using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class CandidateRole_Subscriber_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentRoleProficiencies",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DreamJob",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassionProjectsDescription",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLeaderStyle",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTeamType",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriberLinks",
                columns: table => new
                {
                    SubscriberLinkId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberLinkGuid = table.Column<Guid>(nullable: false),
                    Url = table.Column<string>(maxLength: 2000, nullable: false),
                    Label = table.Column<string>(maxLength: 100, nullable: true),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberLinks", x => x.SubscriberLinkId);
                    table.ForeignKey(
                        name: "FK_SubscriberLinks_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberLinks_SubscriberId",
                table: "SubscriberLinks",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberLinks");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CurrentRoleProficiencies",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "DreamJob",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "PassionProjectsDescription",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "PreferredLeaderStyle",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "PreferredTeamType",
                table: "Subscriber");
        }
    }
}
