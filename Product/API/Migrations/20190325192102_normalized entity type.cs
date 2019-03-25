using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class normalizedentitytype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "SubscriberAction");

            migrationBuilder.AddColumn<int>(
                name: "EntityTypeId",
                table: "SubscriberAction",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EntityType",
                columns: table => new
                {
                    EntityTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EntityTypeGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityType", x => x.EntityTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_EntityTypeId",
                table: "SubscriberAction",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityType_Name",
                table: "EntityType",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction",
                column: "EntityTypeId",
                principalTable: "EntityType",
                principalColumn: "EntityTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberAction_EntityType_EntityTypeId",
                table: "SubscriberAction");

            migrationBuilder.DropTable(
                name: "EntityType");

            migrationBuilder.DropIndex(
                name: "IX_SubscriberAction_EntityTypeId",
                table: "SubscriberAction");

            migrationBuilder.DropColumn(
                name: "EntityTypeId",
                table: "SubscriberAction");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "SubscriberAction",
                nullable: true);
        }
    }
}
