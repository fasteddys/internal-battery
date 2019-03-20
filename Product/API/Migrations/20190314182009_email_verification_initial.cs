using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class email_verification_initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Subscriber",
                nullable: false,
                defaultValue: true);

            // find subscribers that need to verify their email still
            migrationBuilder.Sql(@"
                update Subscriber set IsVerified = 0 where SubscriberId in
                    (SELECT sub.SubscriberId
                        FROM Subscriber sub
	                        inner join SubscriberProfileStagingStore store on store.SubscriberId = sub.SubscriberId
	                    Where ProfileSource = 'CareerCircle')
            ");

            migrationBuilder.CreateTable(
                name: "EmailVerification",
                columns: table => new
                {
                    EmailVerificationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmailVerificationGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    Token = table.Column<Guid>(nullable: false),
                    ExpirationDateTime = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerification", x => x.EmailVerificationId);
                    table.ForeignKey(
                        name: "FK_EmailVerification_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_SubscriberId",
                table: "EmailVerification",
                column: "SubscriberId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerification");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Subscriber");
        }
    }
}
