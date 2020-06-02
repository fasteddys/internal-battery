using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class ReferenceCheckDBModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceCheckStatus",
                schema: "G2",
                columns: table => new
                {
                    ReferenceCheckStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ReferenceCheckStatusGuid = table.Column<Guid>(nullable: false, defaultValueSql: "NewId()"),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCheckStatus", x => x.ReferenceCheckStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceCheckVendor",
                schema: "G2",
                columns: table => new
                {
                    ReferenceCheckVendorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ReferenceCheckVendorGuid = table.Column<Guid>(nullable: false, defaultValueSql: "NewId()"),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCheckVendor", x => x.ReferenceCheckVendorId);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceCheck",
                schema: "G2",
                columns: table => new
                {
                    ReferenceCheckId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ReferenceCheckGuid = table.Column<Guid>(nullable: false, defaultValueSql: "NewId()"),
                    ReferenceCheckRequestId = table.Column<string>(nullable: true),
                    ProfileId = table.Column<int>(nullable: false),
                    ReferenceCheckStatusId = table.Column<int>(nullable: false),
                    ReferenceCheckVendorId = table.Column<int>(nullable: false),
                    RecruiterId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceCheck", x => x.ReferenceCheckId);
                    table.ForeignKey(
                        name: "FK_ReferenceCheck_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferenceCheck_Recruiter_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiter",
                        principalColumn: "RecruiterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferenceCheck_ReferenceCheckStatus_ReferenceCheckStatusId",
                        column: x => x.ReferenceCheckStatusId,
                        principalSchema: "G2",
                        principalTable: "ReferenceCheckStatus",
                        principalColumn: "ReferenceCheckStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferenceCheck_ReferenceCheckVendor_ReferenceCheckVendorId",
                        column: x => x.ReferenceCheckVendorId,
                        principalSchema: "G2",
                        principalTable: "ReferenceCheckVendor",
                        principalColumn: "ReferenceCheckVendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_ProfileId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_RecruiterId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "UIX_ReferenceCheck_ReferenceCheckGuid",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ReferenceCheckGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_ReferenceCheckStatusId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ReferenceCheckStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceCheck_ReferenceCheckVendorId",
                schema: "G2",
                table: "ReferenceCheck",
                column: "ReferenceCheckVendorId");

            migrationBuilder.CreateIndex(
                name: "UIX_ReferenceCheckStatus_Name",
                schema: "G2",
                table: "ReferenceCheckStatus",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_ReferenceCheckStatus_ReferenceCheckStatusGuid",
                schema: "G2",
                table: "ReferenceCheckStatus",
                column: "ReferenceCheckStatusGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_ReferenceCheckVendor_Name",
                schema: "G2",
                table: "ReferenceCheckVendor",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UIX_ReferenceCheckVendor_ReferenceCheckVendorGuid",
                schema: "G2",
                table: "ReferenceCheckVendor",
                column: "ReferenceCheckVendorGuid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceCheck",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ReferenceCheckStatus",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ReferenceCheckVendor",
                schema: "G2");
        }
    }
}
