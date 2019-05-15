using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobdataminingrequiredfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_JobSite_Name",
                table: "JobSite");

            migrationBuilder.DropIndex(
                name: "UIX_JobPage_JobSite_UniqueIdentifier",
                table: "JobPage");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "JobSite",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "JobSite",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "JobPage",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UniqueIdentifier",
                table: "JobPage",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RawData",
                table: "JobPage",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UIX_JobSite_Name",
                table: "JobSite",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_JobPage_JobSite_UniqueIdentifier",
                table: "JobPage",
                columns: new[] { "UniqueIdentifier", "JobSiteId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_JobSite_Name",
                table: "JobSite");

            migrationBuilder.DropIndex(
                name: "UIX_JobPage_JobSite_UniqueIdentifier",
                table: "JobPage");

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "JobSite",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "JobSite",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Uri",
                table: "JobPage",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "UniqueIdentifier",
                table: "JobPage",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "RawData",
                table: "JobPage",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "UIX_JobSite_Name",
                table: "JobSite",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UIX_JobPage_JobSite_UniqueIdentifier",
                table: "JobPage",
                columns: new[] { "UniqueIdentifier", "JobSiteId" },
                unique: true,
                filter: "[UniqueIdentifier] IS NOT NULL");
        }
    }
}
