using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class removedpromocodestabletotakeofs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromoCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromoCode",
                columns: table => new
                {
                    PromoCodesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCode = table.Column<string>(nullable: false),
                    PromoCodesGuid = table.Column<Guid>(nullable: true),
                    PromoDescription = table.Column<string>(nullable: true),
                    PromoEndDate = table.Column<DateTime>(nullable: false),
                    PromoName = table.Column<string>(nullable: false),
                    PromoStartDate = table.Column<DateTime>(nullable: false),
                    PromoTypeId = table.Column<int>(nullable: false),
                    PromoValueFactor = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCode", x => x.PromoCodesId);
                });
        }
    }
}
