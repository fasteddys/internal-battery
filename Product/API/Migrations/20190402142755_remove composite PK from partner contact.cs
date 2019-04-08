using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removecompositePKfrompartnercontact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerContactLeadStatus_PartnerContact_PartnerContactPartnerId_PartnerContactContactId",
                table: "PartnerContactLeadStatus");

            migrationBuilder.DropIndex(
                name: "IX_PartnerContactLeadStatus_PartnerContactPartnerId_PartnerContactContactId",
                table: "PartnerContactLeadStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerContact",
                table: "PartnerContact");

            migrationBuilder.DropColumn(
                name: "PartnerContactContactId",
                table: "PartnerContactLeadStatus");

            migrationBuilder.DropColumn(
                name: "PartnerContactPartnerId",
                table: "PartnerContactLeadStatus");

            migrationBuilder.AddColumn<int>(
                name: "PartnerContactId",
                table: "PartnerContact",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerContact",
                table: "PartnerContact",
                column: "PartnerContactId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContact_PartnerId",
                table: "PartnerContact",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerContactLeadStatus_PartnerContact_PartnerContactId",
                table: "PartnerContactLeadStatus",
                column: "PartnerContactId",
                principalTable: "PartnerContact",
                principalColumn: "PartnerContactId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerContactLeadStatus_PartnerContact_PartnerContactId",
                table: "PartnerContactLeadStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerContact",
                table: "PartnerContact");

            migrationBuilder.DropIndex(
                name: "IX_PartnerContact_PartnerId",
                table: "PartnerContact");

            migrationBuilder.DropColumn(
                name: "PartnerContactId",
                table: "PartnerContact");

            migrationBuilder.AddColumn<int>(
                name: "PartnerContactContactId",
                table: "PartnerContactLeadStatus",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerContactPartnerId",
                table: "PartnerContactLeadStatus",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerContact",
                table: "PartnerContact",
                columns: new[] { "PartnerId", "ContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactLeadStatus_PartnerContactPartnerId_PartnerContactContactId",
                table: "PartnerContactLeadStatus",
                columns: new[] { "PartnerContactPartnerId", "PartnerContactContactId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerContactLeadStatus_PartnerContact_PartnerContactPartnerId_PartnerContactContactId",
                table: "PartnerContactLeadStatus",
                columns: new[] { "PartnerContactPartnerId", "PartnerContactContactId" },
                principalTable: "PartnerContact",
                principalColumns: new[] { "PartnerId", "ContactId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
