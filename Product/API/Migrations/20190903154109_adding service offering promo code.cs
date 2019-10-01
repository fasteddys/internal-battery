using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingserviceofferingpromocode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOfferingPromoCode",
                columns: table => new
                {
                    ServiceOfferingPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ServiceOfferingPromoCodeGuid = table.Column<Guid>(nullable: true),
                    ServiceOfferingId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false),
                    MaxAllowedNumberOfRedemptions = table.Column<int>(nullable: true),
                    NumberOfRedemptions = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOfferingPromoCode", x => x.ServiceOfferingPromoCodeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOfferingPromoCode");
        }
    }
}
