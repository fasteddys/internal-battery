using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifiedpartnercontactfileleadstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContactFile_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContact_PartnerContactId",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerContactFileLeadStatus",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.DropIndex(
                name: "IX_PartnerContactFileLeadStatus_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.DropColumn(
                name: "PartnerContactId",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerContactFileLeadStatus",
                table: "PartnerContactFileLeadStatus",
                columns: new[] { "PartnerContactFileId", "LeadStatusId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContactFile_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                column: "PartnerContactFileId",
                principalTable: "PartnerContactFile",
                principalColumn: "PartnerContactFileId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContactFile_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerContactFileLeadStatus",
                table: "PartnerContactFileLeadStatus");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "PartnerContactId",
                table: "PartnerContactFileLeadStatus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerContactFileLeadStatus",
                table: "PartnerContactFileLeadStatus",
                columns: new[] { "PartnerContactId", "LeadStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactFileLeadStatus_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                column: "PartnerContactFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContactFile_PartnerContactFileId",
                table: "PartnerContactFileLeadStatus",
                column: "PartnerContactFileId",
                principalTable: "PartnerContactFile",
                principalColumn: "PartnerContactFileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerContactFileLeadStatus_PartnerContact_PartnerContactId",
                table: "PartnerContactFileLeadStatus",
                column: "PartnerContactId",
                principalTable: "PartnerContact",
                principalColumn: "PartnerContactId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
