using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class TraitifyCourseTopicBlendMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraitifyBlendCourseTopicMapping",
                columns: table => new
                {
                    TraitifyCourseTopicBlendMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TraitifyCourseTopicBlendMappingGuid = table.Column<Guid>(nullable: false),
                    PersonalityTypeOne = table.Column<string>(nullable: true),
                    PersonalityTypeTwo = table.Column<string>(nullable: true),
                    TopicOneName = table.Column<string>(nullable: true),
                    TopicOneUrl = table.Column<string>(nullable: true),
                    TopicOneImgUrl = table.Column<string>(nullable: true),
                    TopicTwoName = table.Column<string>(nullable: true),
                    TopicTwoUrl = table.Column<string>(nullable: true),
                    TopicTwoImgUrl = table.Column<string>(nullable: true),
                    TopicThreeName = table.Column<string>(nullable: true),
                    TopicThreeUrl = table.Column<string>(nullable: true),
                    TopicThreeImgUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraitifyBlendCourseTopicMapping", x => x.TraitifyCourseTopicBlendMappingId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraitifyBlendCourseTopicMapping");
        }
    }
}
