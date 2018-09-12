using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedrequiredsforSubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GenderID",
                table: "Subscriber",
                newName: "GenderId");

            migrationBuilder.RenameColumn(
                name: "EducationLevelID",
                table: "Subscriber",
                newName: "EducationLevelId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriberGuid",
                table: "Subscriber",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "Subscriber",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "EducationLevelId",
                table: "Subscriber",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Subscriber",
                nullable: true,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GenderId",
                table: "Subscriber",
                newName: "GenderID");

            migrationBuilder.RenameColumn(
                name: "EducationLevelId",
                table: "Subscriber",
                newName: "EducationLevelID");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriberGuid",
                table: "Subscriber",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GenderID",
                table: "Subscriber",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EducationLevelID",
                table: "Subscriber",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Subscriber",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
