using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class alterredEnrollment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnrollmentFee",
                table: "Enrollment",
                newName: "PricePaid");

            migrationBuilder.RenameColumn(
                name: "EnrollDate",
                table: "Enrollment",
                newName: "ModifyDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "Enrollment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Enrollment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Enrollment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DroppedDate",
                table: "Enrollment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "Enrollment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentGuid",
                table: "Enrollment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsRetake",
                table: "Enrollment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Enrollment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PercentComplete",
                table: "Enrollment",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "DroppedDate",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "EnrollmentGuid",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsRetake",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "PercentComplete",
                table: "Enrollment");

            migrationBuilder.RenameColumn(
                name: "PricePaid",
                table: "Enrollment",
                newName: "EnrollmentFee");

            migrationBuilder.RenameColumn(
                name: "ModifyDate",
                table: "Enrollment",
                newName: "EnrollDate");
        }
    }
}
