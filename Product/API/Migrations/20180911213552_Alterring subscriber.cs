using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Alterringsubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /**
            migrationBuilder.RenameColumn(
                name: "MsalObjectId",
                table: "Subscriber",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Subscriber",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateGuid",
                table: "Subscriber",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Subscriber",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Subscriber",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Subscriber",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Subscriber",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LevelOfEducation",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Subscriber",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifyGuid",
                table: "Subscriber",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Subscriber",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberGuid",
                table: "Subscriber",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeEarnedGuid",
                table: "BadgeEarned",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BadgeEarnedID",
                table: "BadgeEarned",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "BadgeCourseGuid",
                table: "BadgeCourse",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BadgeCourseID",
                table: "BadgeCourse",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "CreateGuid",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "LevelOfEducation",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "ModifyGuid",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Subscriber");

            migrationBuilder.DropColumn(
                name: "SubscriberGuid",
                table: "Subscriber");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Subscriber",
                newName: "MsalObjectId");

            migrationBuilder.AlterColumn<string>(
                name: "BadgeEarnedGuid",
                table: "BadgeEarned",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<string>(
                name: "BadgeEarnedID",
                table: "BadgeEarned",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "BadgeCourseGuid",
                table: "BadgeCourse",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<string>(
                name: "BadgeCourseID",
                table: "BadgeCourse",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }
    }
}
