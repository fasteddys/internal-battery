using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class B2Bschemachangeshiringmanageronly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Company",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeSize",
                table: "Company",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndustryId",
                table: "Company",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Company",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HiringManagers",
                schema: "G2",
                columns: table => new
                {
                    HiringManagerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    HiringManagerGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringManagers", x => x.HiringManagerId);
                    table.ForeignKey(
                        name: "FK_HiringManagers_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringManagers_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Company_IndustryId",
                table: "Company",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringManagers_CompanyId",
                schema: "G2",
                table: "HiringManagers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringManagers_SubscriberId",
                schema: "G2",
                table: "HiringManagers",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Industry_IndustryId",
                table: "Company",
                column: "IndustryId",
                principalTable: "Industry",
                principalColumn: "IndustryId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Industry_IndustryId",
                table: "Company");

            migrationBuilder.DropTable(
                name: "HiringManagers",
                schema: "G2");

            migrationBuilder.DropIndex(
                name: "IX_Company_IndustryId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "EmployeeSize",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Company");
        }
    }
}
