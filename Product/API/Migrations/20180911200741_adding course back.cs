using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingcourseback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<int>(nullable: false),
                    CourseCode = table.Column<string>(nullable: false),
                    CourseSchedule = table.Column<int>(nullable: false),
                    PurchasePrice = table.Column<decimal>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Course");
        }
    }
}
