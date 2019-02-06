using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class added_terms_to_rebate_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Terms",
                table: "RebateType",
                nullable: true);

            migrationBuilder.UpdateData("RebateType", "RebateTypeId", 1, "Terms", "Pour-over listicle stumptown edison bulb readymade yr food truck kinfolk kombucha af you probably haven't heard of them literally. Tumeric four loko intelligentsia la croix brunch live-edge chartreuse distillery. Messenger bag direct trade DIY meggings shabby chic, PBR&B offal umami bushwick palo santo. La croix celiac meditation pug ramps XOXO lumbersexual health goth succulents subway tile cred. Af kombucha bespoke tbh, godard meggings hammock hella chartreuse roof party.");
            migrationBuilder.UpdateData("RebateType", "RebateTypeId", 2, "Terms", "Capitalize on low hanging fruit to identify a ballpark value added activity to beta test. Override the digital divide with additional clickthroughs from DevOps. Nanotechnology immersion along the information highway will close the loop on focusing solely on the bottom line.");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Terms",
                table: "RebateType");
        }
    }
}
