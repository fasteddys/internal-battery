using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class renamedjsonpropertyforjobalert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobSearchDtoJSON",
                table: "JobPostingAlert",
                newName: "JobQueryDtoJSON");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobQueryDtoJSON",
                table: "JobPostingAlert",
                newName: "JobSearchDtoJSON");
        }
    }
}
