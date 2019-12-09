using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subsciber_topic_and_course_level : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseLevelId",
                table: "Course",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseLevel",
                columns: table => new
                {
                    CourseLevelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseLevelGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLevel", x => x.CourseLevelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_TopicId",
                table: "Subscriber",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_CourseLevelId",
                table: "Course",
                column: "CourseLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_CourseLevel_CourseLevelId",
                table: "Course",
                column: "CourseLevelId",
                principalTable: "CourseLevel",
                principalColumn: "CourseLevelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriber_Topic_TopicId",
                table: "Subscriber",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_CourseLevel_CourseLevelId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriber_Topic_TopicId",
                table: "Subscriber");

            migrationBuilder.DropTable(
                name: "CourseLevel");

            migrationBuilder.DropIndex(
                name: "IX_Subscriber_TopicId",
                table: "Subscriber");

            migrationBuilder.DropIndex(
                name: "IX_Course_CourseLevelId",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CourseLevelId",
                table: "Course");
        }
    }
}
