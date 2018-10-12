using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingbackpromocodetable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromoCode",
                columns: table => new
                {
                    PromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCodeGuid = table.Column<Guid>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    PromoStartDate = table.Column<DateTime>(nullable: false),
                    PromoEndDate = table.Column<DateTime>(nullable: false),
                    PromoTypeId = table.Column<int>(nullable: false),
                    PromoValueFactor = table.Column<decimal>(nullable: false),
                    PromoName = table.Column<string>(nullable: false),
                    PromoDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCode", x => x.PromoCodeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromoCode");
        }
    }
}
