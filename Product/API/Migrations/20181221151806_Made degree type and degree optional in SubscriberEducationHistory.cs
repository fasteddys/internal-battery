using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class MadedegreetypeanddegreeoptionalinSubscriberEducationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegree_EducationalDegreeId",
                table: "SubscriberEducationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegreeType_EducationalDegreeTypeId",
                table: "SubscriberEducationHistory");

            migrationBuilder.AlterColumn<int>(
                name: "EducationalDegreeTypeId",
                table: "SubscriberEducationHistory",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "EducationalDegreeId",
                table: "SubscriberEducationHistory",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegree_EducationalDegreeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeId",
                principalTable: "EducationalDegree",
                principalColumn: "EducationalDegreeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegreeType_EducationalDegreeTypeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeTypeId",
                principalTable: "EducationalDegreeType",
                principalColumn: "EducationalDegreeTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegree_EducationalDegreeId",
                table: "SubscriberEducationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegreeType_EducationalDegreeTypeId",
                table: "SubscriberEducationHistory");

            migrationBuilder.AlterColumn<int>(
                name: "EducationalDegreeTypeId",
                table: "SubscriberEducationHistory",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EducationalDegreeId",
                table: "SubscriberEducationHistory",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegree_EducationalDegreeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeId",
                principalTable: "EducationalDegree",
                principalColumn: "EducationalDegreeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriberEducationHistory_EducationalDegreeType_EducationalDegreeTypeId",
                table: "SubscriberEducationHistory",
                column: "EducationalDegreeTypeId",
                principalTable: "EducationalDegreeType",
                principalColumn: "EducationalDegreeTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
