using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ReferenceCheckDBModelupdates1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferenceCheck_CandidateReference_CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropForeignKey(
                name: "FK_ReferenceCheck_ReferenceCheckStatus_ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceCheck_CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceCheck_ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropColumn(
                name: "CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.DropColumn(
                name: "ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "G2",
                table: "ReferenceCheckStatus",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "G2",
                table: "CandidateReference",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheckStatus_ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus",
                column: "ReferenceCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReference_ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference",
                column: "ReferenceCheckId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateReference_ReferenceCheck_ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference",
                column: "ReferenceCheckId",
                principalSchema: "G2",
                principalTable: "ReferenceCheck",
                principalColumn: "ReferenceCheckId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReferenceCheckStatus_ReferenceCheck_ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus",
                column: "ReferenceCheckId",
                principalSchema: "G2",
                principalTable: "ReferenceCheck",
                principalColumn: "ReferenceCheckId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateReference_ReferenceCheck_ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference");

            migrationBuilder.DropForeignKey(
                name: "FK_ReferenceCheckStatus_ReferenceCheck_ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceCheckStatus_ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus");

            migrationBuilder.DropIndex(
                name: "IX_CandidateReference_ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference");

            migrationBuilder.DropColumn(
                name: "ReferenceCheckId",
                schema: "G2",
                table: "ReferenceCheckStatus");

            migrationBuilder.DropColumn(
                name: "ReferenceCheckId",
                schema: "G2",
                table: "CandidateReference");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "G2",
                table: "CandidateReference");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "G2",
                table: "ReferenceCheckStatus",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "CandidateReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ReferenceCheckStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferenceCheck_CandidateReference_CandidateReferenceId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "CandidateReferenceId",
                principalSchema: "G2",
                principalTable: "CandidateReference",
                principalColumn: "CandidateReferenceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReferenceCheck_ReferenceCheckStatus_ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ReferenceCheckStatusId",
                principalSchema: "G2",
                principalTable: "ReferenceCheckStatus",
                principalColumn: "ReferenceCheckStatusId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
