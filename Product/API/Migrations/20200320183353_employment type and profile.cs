using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class employmenttypeandprofile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_EmploymentType_EmploymentTypeId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_EmploymentTypeId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.CreateTable(
                name: "ProfileEmploymentTypes",
                schema: "G2",
                columns: table => new
                {
                    ProfileEmploymentTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileEmploymentTypeGuid = table.Column<Guid>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    EmploymentTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileEmploymentTypes", x => x.ProfileEmploymentTypeId);
                    table.ForeignKey(
                        name: "FK_ProfileEmploymentTypes_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalTable: "EmploymentType",
                        principalColumn: "EmploymentTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileEmploymentTypes_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileEmploymentTypes_EmploymentTypeId",
                schema: "G2",
                table: "ProfileEmploymentTypes",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "UIX_ProfileEmploymentType_Profile_EmploymentType",
                schema: "G2",
                table: "ProfileEmploymentTypes",
                columns: new[] { "ProfileId", "EmploymentTypeId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileEmploymentTypes",
                schema: "G2");

            migrationBuilder.AddColumn<int>(
                name: "EmploymentTypeId",
                schema: "G2",
                table: "Profiles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_EmploymentTypeId",
                schema: "G2",
                table: "Profiles",
                column: "EmploymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_EmploymentType_EmploymentTypeId",
                schema: "G2",
                table: "Profiles",
                column: "EmploymentTypeId",
                principalTable: "EmploymentType",
                principalColumn: "EmploymentTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}