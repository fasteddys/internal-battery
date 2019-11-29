using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class custompasswordreset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetRequest",
                columns: table => new
                {
                    PasswordResetRequestId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PasswordResetRequestGuid = table.Column<Guid>(nullable: false),
                    ResetDate = table.Column<DateTime>(nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    RedemptionStatusId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetRequest", x => x.PasswordResetRequestId);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequest_RedemptionStatus_RedemptionStatusId",
                        column: x => x.RedemptionStatusId,
                        principalTable: "RedemptionStatus",
                        principalColumn: "RedemptionStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequest_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequest_RedemptionStatusId",
                table: "PasswordResetRequest",
                column: "RedemptionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequest_SubscriberId",
                table: "PasswordResetRequest",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetRequest");
        }
    }
}
