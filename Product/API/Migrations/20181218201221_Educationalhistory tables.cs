using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Educationalhistorytables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationalDegree",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationalDegreeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EducationalDegreeGuid = table.Column<Guid>(nullable: false),
                    Degree = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalDegree", x => x.EducationalDegreeId);
                });

            migrationBuilder.CreateTable(
                name: "EducationalDegreeType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationalDegreeTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EducationalDegreeTypeGuid = table.Column<Guid>(nullable: false),
                    DegreeType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalDegreeType", x => x.EducationalDegreeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EducationalInstitution",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationalInstitutionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EducationalInstitutionGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalInstitution", x => x.EducationalInstitutionId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberEducationHistory",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberEducationHistoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberEducationHistoryGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    EducationalInstitutionId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    DegreeDate = table.Column<DateTime>(nullable: false),
                    EducationalDegreeTypeId = table.Column<int>(nullable: false),
                    EducationalDegreeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberEducationHistory", x => x.SubscriberEducationHistoryId);
                    table.ForeignKey(
                        name: "FK_SubscriberEducationHistory_EducationalDegree_EducationalDegreeId",
                        column: x => x.EducationalDegreeId,
                        principalTable: "EducationalDegree",
                        principalColumn: "EducationalDegreeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberEducationHistory_EducationalDegreeType_EducationalDegreeTypeId",
                        column: x => x.EducationalDegreeTypeId,
                        principalTable: "EducationalDegreeType",
                        principalColumn: "EducationalDegreeTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberEducationHistory_EducationalInstitution_EducationalInstitutionId",
                        column: x => x.EducationalInstitutionId,
                        principalTable: "EducationalInstitution",
                        principalColumn: "EducationalInstitutionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberEducationHistory_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberEducationHistory_EducationalDegreeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberEducationHistory_EducationalDegreeTypeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberEducationHistory_EducationalInstitutionId",
                table: "SubscriberEducationHistory",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberEducationHistory_SubscriberId",
                table: "SubscriberEducationHistory",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberEducationHistory");

            migrationBuilder.DropTable(
                name: "EducationalDegree");

            migrationBuilder.DropTable(
                name: "EducationalDegreeType");

            migrationBuilder.DropTable(
                name: "EducationalInstitution");
        }
    }
}
