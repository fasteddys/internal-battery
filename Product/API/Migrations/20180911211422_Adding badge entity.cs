using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Addingbadgeentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /**migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Course",
                nullable: false,
                oldClrType: typeof(Guid))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);*/

            migrationBuilder.AddColumn<Guid>(
                name: "CourseGuid",
                table: "Course",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Course",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Badge",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: false),
                    BadgeID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeGuid = table.Column<Guid>(nullable: false),
                    BadgeName = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Points = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: false),
                    SortOrder = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badge", x => x.BadgeID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Badge");

            migrationBuilder.DropColumn(
                name: "CourseGuid",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Course");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Course",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }
    }
}
