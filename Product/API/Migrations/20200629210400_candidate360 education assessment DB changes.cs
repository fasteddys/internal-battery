using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class candidate360educationassessmentDBchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "RelevantYear",
                table: "SubscriberEducationHistory",
                type: "SmallInt",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Skill",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldDefaultValueSql: "1");

            migrationBuilder.AddColumn<int>(
                name: "EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EducationalDegreeTypeCategory",
                columns: table => new
                {
                    EducationalDegreeTypeCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationalDegreeTypeCategoryGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalDegreeTypeCategory", x => x.EducationalDegreeTypeCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TrainingType",
                columns: table => new
                {
                    TrainingTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TrainingTypeGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingType", x => x.TrainingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberTraining",
                columns: table => new
                {
                    SubscriberTrainingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberTrainingGuid = table.Column<Guid>(nullable: false),
                    TrainingTypeId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    TrainingInstitution = table.Column<string>(type: "Varchar(150)", maxLength: 150, nullable: true),
                    TrainingName = table.Column<string>(type: "Varchar(150)", maxLength: 150, nullable: true),
                    RelevantYear = table.Column<short>(type: "SmallInt", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberTraining", x => x.SubscriberTrainingId);
                    table.ForeignKey(
                        name: "FK_SubscriberTraining_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberTraining_TrainingType_TrainingTypeId",
                        column: x => x.TrainingTypeId,
                        principalTable: "TrainingType",
                        principalColumn: "TrainingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalDegreeType_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                column: "EducationalDegreeTypeCategoryId");

            migrationBuilder.CreateIndex(
                name: "UIX_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryGuid",
                table: "EducationalDegreeTypeCategory",
                column: "EducationalDegreeTypeCategoryGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberTraining_SubscriberId",
                table: "SubscriberTraining",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "UIX_SubscriberTraining_SubscriberTrainingGuid",
                table: "SubscriberTraining",
                column: "SubscriberTrainingGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberTraining_TrainingTypeId",
                table: "SubscriberTraining",
                column: "TrainingTypeId");

            migrationBuilder.CreateIndex(
                name: "UIX_TrainingType_TrainingTypeGuid",
                table: "TrainingType",
                column: "TrainingTypeGuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalDegreeType_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                column: "EducationalDegreeTypeCategoryId",
                principalTable: "EducationalDegreeTypeCategory",
                principalColumn: "EducationalDegreeTypeCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalDegreeType_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropTable(
                name: "EducationalDegreeTypeCategory");

            migrationBuilder.DropTable(
                name: "SubscriberTraining");

            migrationBuilder.DropTable(
                name: "TrainingType");

            migrationBuilder.DropIndex(
                name: "IX_EducationalDegreeType_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "RelevantYear",
                table: "SubscriberEducationHistory");

            migrationBuilder.DropColumn(
                name: "EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "EducationalDegreeType");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Skill",
                nullable: false,
                defaultValueSql: "1",
                oldClrType: typeof(bool),
                oldNullable: true,
                oldDefaultValue: true);
        }
    }
}
