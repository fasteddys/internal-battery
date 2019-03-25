using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscriberactiontracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriberAction",
                columns: table => new
                {
                    SubscriberId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberActionGuid = table.Column<Guid>(nullable: false),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EntityId = table.Column<int>(nullable: false),
                    EntityType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberAction", x => new { x.SubscriberId, x.ActionId });
                    table.ForeignKey(
                        name: "FK_SubscriberAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberAction_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAction_ActionId",
                table: "SubscriberAction",
                column: "ActionId");


            migrationBuilder.InsertData(
                table: "Action",
                columns: new[] { "ActionId", "ActionGuid", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name" },
                values: new object[,]
                {
                    { 6, new Guid("BDE22CB0-D811-4928-A4E1-F3DC01D63D9C"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "View subscriber" },
                    { 7, new Guid("73F10844-AAD7-4A2D-A03A-82E786A27B58"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Download resume" },
                    { 8, new Guid("7AA2AE38-EA9A-41DE-82A0-DA8B8E2F7A60"), new DateTime(2019, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), null, 0, null, null, "Partner offer" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberAction");
        }
    }
}
