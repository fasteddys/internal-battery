using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addingsubscribergroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    GroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    GroupGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsLeavable = table.Column<int>(nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "GroupPartner",
                columns: table => new
                {
                    GroupPartnerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    GroupPartnerGuid = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    PartnerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPartner", x => x.GroupPartnerId);
                    table.ForeignKey(
                        name: "FK_GroupPartner_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPartner_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberGroup",
                columns: table => new
                {
                    SubscriberGroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberGroupGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberGroup", x => x.SubscriberGroupId);
                    table.ForeignKey(
                        name: "FK_SubscriberGroup_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberGroup_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPartner_GroupId",
                table: "GroupPartner",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPartner_PartnerId",
                table: "GroupPartner",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberGroup_GroupId",
                table: "SubscriberGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberGroup_SubscriberId",
                table: "SubscriberGroup",
                column: "SubscriberId");

            migrationBuilder.InsertData(
                table: "Group",
                columns: new[] { "IsDeleted", "GroupGuid", "Name", "CreateDate", "CreateGuid", "Description" },
                values: new object[,]
                {
                    { 0, Guid.NewGuid(), "Coursera Signup",  new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account via a Coursera campaign."},
                    { 0, Guid.NewGuid(), "Barnett Signup", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account via a Barnett International campaign."},
                    { 0, Guid.NewGuid(), "ITProTV Signup", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account via an ITProTV campaign."},
                    { 0, Guid.NewGuid(), "Woz Student", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a WozU course."},
                    { 0, Guid.NewGuid(), "NEXXT Lead Signup", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account via a NEXXT lead."},
                    { 0, Guid.NewGuid(), "Mom Project Signup", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account via a Mom Project campaign."},
                    { 0, Guid.NewGuid(), "RWS Intake", new DateTime(2019, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), Guid.Empty, "Subscriber who has signed up for a CareerCircle account after being retrieved from the RWS job scrape."}
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPartner");

            migrationBuilder.DropTable(
                name: "SubscriberGroup");

            migrationBuilder.DropTable(
                name: "Group");
        }
    }
}
