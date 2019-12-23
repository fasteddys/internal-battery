using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingCourseReferralmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseReferral",
                columns: table => new
                {
                    CourseReferralId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseReferralGuid = table.Column<Guid>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    ReferrerId = table.Column<int>(nullable: false),
                    RefereeId = table.Column<int>(nullable: true),
                    RefereeEmail = table.Column<string>(nullable: true),
                    IsCourseViewed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReferral", x => x.CourseReferralId);
                    table.ForeignKey(
                        name: "FK_CourseReferral_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CourseReferral_Subscriber_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CourseReferral_Subscriber_ReferrerId",
                        column: x => x.ReferrerId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseReferral_CourseId",
                table: "CourseReferral",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReferral_RefereeId",
                table: "CourseReferral",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReferral_ReferrerId",
                table: "CourseReferral",
                column: "ReferrerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseReferral");
        }
    }
}
