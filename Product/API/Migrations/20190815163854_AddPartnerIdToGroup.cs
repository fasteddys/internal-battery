using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddPartnerIdToGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "Group",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group_PartnerId",
                table: "Group",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Partner_PartnerId",
                table: "Group",
                column: "PartnerId",
                principalTable: "Partner",
                principalColumn: "PartnerId",
                onDelete: ReferentialAction.Restrict);

             migrationBuilder.CreateIndex(
                name:"UX_Group_Partner",
                table:"Group",
                columns: new[] {"GroupId","PartnerId"},
                unique:true 
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_Partner_PartnerId",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Group_PartnerId",
                table: "Group");
            
            migrationBuilder.DropIndex(
                name:"UX_Group_Partner",
                table:"Group"
            );

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Group");
        }
    }
}
