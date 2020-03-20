using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modificationstoprofilecomments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "G2",
                table: "ProfileComments",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecruiterId",
                schema: "G2",
                table: "ProfileComments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileComments_RecruiterId",
                schema: "G2",
                table: "ProfileComments",
                column: "RecruiterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileComments_Recruiter_RecruiterId",
                schema: "G2",
                table: "ProfileComments",
                column: "RecruiterId",
                principalTable: "Recruiter",
                principalColumn: "RecruiterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileComments_Recruiter_RecruiterId",
                schema: "G2",
                table: "ProfileComments");

            migrationBuilder.DropIndex(
                name: "IX_ProfileComments_RecruiterId",
                schema: "G2",
                table: "ProfileComments");

            migrationBuilder.DropColumn(
                name: "RecruiterId",
                schema: "G2",
                table: "ProfileComments");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "G2",
                table: "ProfileComments",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 500);
        }
    }
}
