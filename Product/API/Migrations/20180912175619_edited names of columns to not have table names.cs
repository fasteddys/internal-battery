using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class editednamesofcolumnstonothavetablenames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorName",
                table: "Vendor",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TopicName",
                table: "Topic",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "Tag",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "EnrollmentDate",
                table: "Enrollment",
                newName: "DateEnrolled");

            migrationBuilder.RenameColumn(
                name: "CourseName",
                table: "Course",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CourseDescription",
                table: "Course",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CourseCode",
                table: "Course",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "BadgeSetName",
                table: "BadgeSet",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "BadgeSetDescription",
                table: "BadgeSet",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "BadgeName",
                table: "Badge",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Vendor",
                newName: "VendorName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Topic",
                newName: "TopicName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tag",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "DateEnrolled",
                table: "Enrollment",
                newName: "EnrollmentDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Course",
                newName: "CourseName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Course",
                newName: "CourseDescription");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Course",
                newName: "CourseCode");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "BadgeSet",
                newName: "BadgeSetName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "BadgeSet",
                newName: "BadgeSetDescription");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Badge",
                newName: "BadgeName");
        }
    }
}
