using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addedpaymenttables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PromoCodeId",
                table: "SubscriberPromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RebateAmount",
                table: "Rebate",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RebateIssueDate",
                table: "Rebate",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RebateIssueStatus",
                table: "Rebate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RebateIssued",
                table: "Rebate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PromoTypeName",
                table: "PromoType",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PromoName",
                table: "PromoCode",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PromoCode",
                table: "PromoCode",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromoCodeId",
                table: "CoursePromoCode",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EnrollmentStatus",
                columns: table => new
                {
                    EnrollmentStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentStatusGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentStatus", x => x.EnrollmentStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentGuid = table.Column<Guid>(nullable: true),
                    PaymentNonce = table.Column<string>(nullable: true),
                    PaymentValue = table.Column<decimal>(nullable: false),
                    PaymentCurrencyType = table.Column<string>(nullable: true),
                    PaymentDate = table.Column<DateTime>(nullable: false),
                    PaymentStatus = table.Column<int>(nullable: false),
                    PaymentProcessorId = table.Column<int>(nullable: false),
                    EnrollmentId = table.Column<int>(nullable: false),
                    PaymentBatchId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentBatch",
                columns: table => new
                {
                    PaymentBatchId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentBatchGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentBatch", x => x.PaymentBatchId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProcessor",
                columns: table => new
                {
                    PaymentProcessorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentProcessorGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProcessor", x => x.PaymentProcessorId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatus",
                columns: table => new
                {
                    PaymentStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentStatusGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatus", x => x.PaymentStatusId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentStatus");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentBatch");

            migrationBuilder.DropTable(
                name: "PaymentProcessor");

            migrationBuilder.DropTable(
                name: "PaymentStatus");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                table: "SubscriberPromoCode");

            migrationBuilder.DropColumn(
                name: "RebateAmount",
                table: "Rebate");

            migrationBuilder.DropColumn(
                name: "RebateIssueDate",
                table: "Rebate");

            migrationBuilder.DropColumn(
                name: "RebateIssueStatus",
                table: "Rebate");

            migrationBuilder.DropColumn(
                name: "RebateIssued",
                table: "Rebate");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                table: "CoursePromoCode");

            migrationBuilder.AlterColumn<string>(
                name: "PromoTypeName",
                table: "PromoType",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "PromoName",
                table: "PromoCode",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "PromoCode",
                table: "PromoCode",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
