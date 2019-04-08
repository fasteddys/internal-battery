using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class leaddatacapture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "PartnerContact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Partner",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerTypeId",
                table: "Partner",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeadStatus",
                columns: table => new
                {
                    LeadStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    LeadStatusGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Severity = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadStatus", x => x.LeadStatusId);
                });

            migrationBuilder.CreateTable(
                name: "PartnerType",
                columns: table => new
                {
                    PartnerTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerTypeGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerType", x => x.PartnerTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContactLeadStatus",
                columns: table => new
                {
                    PartnerContactId = table.Column<int>(nullable: false),
                    LeadStatusId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PartnerContactPartnerId = table.Column<int>(nullable: true),
                    PartnerContactContactId = table.Column<int>(nullable: true),
                    PartnerContactLeadStatusGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactLeadStatus", x => new { x.PartnerContactId, x.LeadStatusId });
                    table.ForeignKey(
                        name: "FK_PartnerContactLeadStatus_LeadStatus_LeadStatusId",
                        column: x => x.LeadStatusId,
                        principalTable: "LeadStatus",
                        principalColumn: "LeadStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerContactLeadStatus_PartnerContact_PartnerContactPartnerId_PartnerContactContactId",
                        columns: x => new { x.PartnerContactPartnerId, x.PartnerContactContactId },
                        principalTable: "PartnerContact",
                        principalColumns: new[] { "PartnerId", "ContactId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partner_PartnerTypeId",
                table: "Partner",
                column: "PartnerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactLeadStatus_LeadStatusId",
                table: "PartnerContactLeadStatus",
                column: "LeadStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactLeadStatus_PartnerContactPartnerId_PartnerContactContactId",
                table: "PartnerContactLeadStatus",
                columns: new[] { "PartnerContactPartnerId", "PartnerContactContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerType_Name",
                table: "PartnerType",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Partner_PartnerType_PartnerTypeId",
                table: "Partner",
                column: "PartnerTypeId",
                principalTable: "PartnerType",
                principalColumn: "PartnerTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.InsertData(
                table: "LeadStatus",
                columns: new[] { "LeadStatusId", "LeadStatusGuid", "Name", "Description", "Severity", "IsDeleted", "CreateDate", "CreateGuid" },
                values: new object[,]
                {
                    { 1, new Guid("8CE6ECDE-E998-462E-8E46-81FFCE91374F"), "System Error", "An internal error has occurred; please contact CareerCircle support", "Unknown", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 2, new Guid("766D6AC0-AC4F-4387-8B7D-2F9B95AAB0BB"), "Inserted", "Lead has been processed successfully and inserted", "Information", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 3, new Guid("F5FBF3AA-8EF3-4A9A-9C86-91C41CCB0F78"), "Duplicate", "Lead has been identified as a duplicate", "Rejected", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 4, new Guid("155DDA40-76B4-4B56-98C1-75C7E4D682C3"), "Test", "Lead has been identified as a test lead", "Warning", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                    { 5, new Guid("52E809BB-3F07-4801-8D62-72C43F628B07"), "Required Fields", "Lead is missing required fields", "Rejected", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") },
                });

            migrationBuilder.InsertData(
                table: "PartnerType",
                columns: new[] { "PartnerTypeId", "PartnerTypeGuid", "Name", "Description", "IsDeleted", "CreateDate", "CreateGuid" },
                values: new object[,]
                {
                    { 1, new Guid("8CE6ECDE-E998-462E-8E46-81FFCE91374F"), "Pay Per Lead", "Online advertising payment model in which payment is based solely on qualifying leads.", 0, new DateTime(2019, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.Sql(@"ALTER TABLE dbo.PartnerContact ADD vMobilePhone AS JSON_VALUE(MetaDataJSON,'$.MobilePhone')");
            migrationBuilder.Sql(@"CREATE INDEX IX_PartnerContact_MetaDataJSON_MobilePhone ON dbo.PartnerContact(vMobilePhone)");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE dbo.PartnerContact DROP vMobilePhone");
            migrationBuilder.Sql(@"DROP INDEX IX_PartnerContact_MetaDataJSON_MobilePhone ON dbo.PartnerContact");

            migrationBuilder.DropForeignKey(
                name: "FK_Partner_PartnerType_PartnerTypeId",
                table: "Partner");

            migrationBuilder.DropTable(
                name: "PartnerContactLeadStatus");

            migrationBuilder.DropTable(
                name: "PartnerType");

            migrationBuilder.DropTable(
                name: "LeadStatus");

            migrationBuilder.DropIndex(
                name: "IX_Partner_PartnerTypeId",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "IsBillable",
                table: "PartnerContact");

            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "PartnerTypeId",
                table: "Partner");
        }
    }
}
