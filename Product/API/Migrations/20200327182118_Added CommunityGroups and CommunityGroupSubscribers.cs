using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddedCommunityGroupsandCommunityGroupSubscribers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityGroup",
                columns: table => new
                {
                    CommunityGroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommunityGroupGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityGroup", x => x.CommunityGroupId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityGroupSubscriber",
                columns: table => new
                {
                    CommunityGroupSubscriberId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommunityGroupSubscriberGuid = table.Column<Guid>(nullable: true),
                    CommunityGroupId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    SubscriberId1 = table.Column<int>(nullable: true),
                    CommunityGroupId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityGroupSubscriber", x => x.CommunityGroupSubscriberId);
                    table.ForeignKey(
                        name: "FK_CommunityGroupSubscriber_CommunityGroup_CommunityGroupId",
                        column: x => x.CommunityGroupId,
                        principalTable: "CommunityGroup",
                        principalColumn: "CommunityGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityGroupSubscriber_CommunityGroup_CommunityGroupId1",
                        column: x => x.CommunityGroupId1,
                        principalTable: "CommunityGroup",
                        principalColumn: "CommunityGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityGroupSubscriber_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityGroupSubscriber_Subscriber_SubscriberId1",
                        column: x => x.SubscriberId1,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroupSubscriber_CommunityGroupId",
                table: "CommunityGroupSubscriber",
                column: "CommunityGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroupSubscriber_CommunityGroupId1",
                table: "CommunityGroupSubscriber",
                column: "CommunityGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroupSubscriber_SubscriberId",
                table: "CommunityGroupSubscriber",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroupSubscriber_SubscriberId1",
                table: "CommunityGroupSubscriber",
                column: "SubscriberId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityGroupSubscriber");

            migrationBuilder.DropTable(
                name: "CommunityGroup");
        }
    }
}
