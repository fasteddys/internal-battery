using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class modifiedwishlistuniqueindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_Wishlist_Recruiter_Name",
                schema: "G2",
                table: "Wishlists");

            migrationBuilder.CreateIndex(
                name: "UIX_Wishlist_Recruiter_Name_IsDeleted",
                schema: "G2",
                table: "Wishlists",
                columns: new[] { "RecruiterId", "Name", "IsDeleted" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UIX_Wishlist_Recruiter_Name_IsDeleted",
                schema: "G2",
                table: "Wishlists");

            migrationBuilder.CreateIndex(
                name: "UIX_Wishlist_Recruiter_Name",
                schema: "G2",
                table: "Wishlists",
                columns: new[] { "RecruiterId", "Name" },
                unique: true);
        }
    }
}
