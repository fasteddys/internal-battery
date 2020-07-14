using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class dropunwantedindexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_TrainingType_TrainingTypeGuid",
                table: "TrainingType");

            migrationBuilder.DropIndex(
                name: "UIX_SubscriberTraining_SubscriberTrainingGuid",
                table: "SubscriberTraining");

            migrationBuilder.DropIndex(
                name: "UIX_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryGuid",
                table: "EducationalDegreeTypeCategory");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UIX_TrainingType_TrainingTypeGuid",
                table: "TrainingType",
                column: "TrainingTypeGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_SubscriberTraining_SubscriberTrainingGuid",
                table: "SubscriberTraining",
                column: "SubscriberTrainingGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryGuid",
                table: "EducationalDegreeTypeCategory",
                column: "EducationalDegreeTypeCategoryGuid",
                unique: true);
        }
    }
}
