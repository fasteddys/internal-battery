using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addC360employmentpreferencesmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommuteDistanceId",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexibleWorkScheduleRequired",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWillingToTravel",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommuteDistance",
                columns: table => new
                {
                    CommuteDistanceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommuteDistanceGuid = table.Column<Guid>(nullable: false),
                    DistanceRange = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommuteDistance", x => x.CommuteDistanceId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberEmploymentTypes",
                columns: table => new
                {
                    SubscriberEmploymentTypesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberEmploymentTypesGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    EmploymentTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberEmploymentTypes", x => x.SubscriberEmploymentTypesId);
                    table.ForeignKey(
                        name: "FK_SubscriberEmploymentTypes_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalTable: "EmploymentType",
                        principalColumn: "EmploymentTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberEmploymentTypes_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_CommuteDistanceId",
                table: "Subscriber",
                column: "CommuteDistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberEmploymentTypes_EmploymentTypeId",
                table: "SubscriberEmploymentTypes",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "UIX_SubscriberEmploymentTypes_Subscriber_EmploymentType",
                table: "SubscriberEmploymentTypes",
                columns: new[] { "SubscriberId", "EmploymentTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriber_CommuteDistance_CommuteDistanceId",
                table: "Subscriber",
                column: "CommuteDistanceId",
                principalTable: "CommuteDistance",
                principalColumn: "CommuteDistanceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"Insert [dbo].[CommuteDistance]
                    Values (0,GETUTCDATE(),Null,'00000000-0000-0000-0000-000000000000', Null,'B13953D8-2E36-4004-A81A-F83D32B1AF76','0 miles'),
                           (0,GETUTCDATE(),Null,'00000000-0000-0000-0000-000000000000', Null,'20A63515-DD45-4CC7-AF73-224BE3814431','1-15 miles'),
                           (0,GETUTCDATE(),Null,'00000000-0000-0000-0000-000000000000', Null,'3D55CD71-5395-482A-BAE9-E357B6C7ABCE','16-30 miles'),
                           (0,GETUTCDATE(),Null,'00000000-0000-0000-0000-000000000000', Null,'CA248AB4-905F-4C75-B920-249480F45B9A','31-50 mile'),
                           (0,GETUTCDATE(),Null,'00000000-0000-0000-0000-000000000000', Null,'F34FA438-C1D7-4221-9B27-2FAE49062C43','51+ miles')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriber_CommuteDistance_CommuteDistanceId",
                table: "Subscriber");

            migrationBuilder.DropTable(
                name: "CommuteDistance");

            migrationBuilder.DropTable(
                name: "SubscriberEmploymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Subscriber_CommuteDistanceId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CommuteDistanceId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "IsFlexibleWorkScheduleRequired",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "IsWillingToTravel",
                table: "Subscriber");
        }
    }
}
