using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class indexstatusinfoforg2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AzureIndexStatusId",
                schema: "G2",
                table: "Profiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureSearchIndexInfo",
                schema: "G2",
                table: "Profiles",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AzureIndexStatuses",
                schema: "G2",
                columns: table => new
                {
                    AzureIndexStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    AzureIndexStatusGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 25, nullable: false),
                    Description = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureIndexStatuses", x => x.AzureIndexStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AzureIndexStatusId",
                schema: "G2",
                table: "Profiles",
                column: "AzureIndexStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_AzureIndexStatuses_AzureIndexStatusId",
                schema: "G2",
                table: "Profiles",
                column: "AzureIndexStatusId",
                principalSchema: "G2",
                principalTable: "AzureIndexStatuses",
                principalColumn: "AzureIndexStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.InsertData(
                   table: "AzureIndexStatuses",
                   columns: new[] { "AzureIndexStatusGuid", "CreateDate", "CreateGuid", "Name", "Description", "IsDeleted" },
                   values: new object[,]
                   {
                       { new Guid("CEFCBD4D-98C5-45E2-B9CA-74ECB57069B6"), new DateTime(2020, 2, 27), new Guid("00000000-0000-0000-0000-000000000000"), "None", "The document has not yet been indexed.", 0 },
                        { new Guid("361B23AB-F882-447B-8AE3-EE581B93BA5B"), new DateTime(2020, 2, 27), new Guid("00000000-0000-0000-0000-000000000000"), "Pending", "A change has been made to a document and it needs to be re-indexed.", 0 },
                        { new Guid("BCB2F576-BFAA-452E-959D-0F0EA652F6C0"), new DateTime(2020, 2, 27), new Guid("00000000-0000-0000-0000-000000000000"), "Indexed", "The document has been indexed since the latest change to the entity was made.", 0 },
                        { new Guid("B510EB03-93EA-43F9-88EA-F1BE5665C137"), new DateTime(2020, 2, 27), new Guid("00000000-0000-0000-0000-000000000000"), "Deleted", "The document has been deleted from the index.", 0 },
                        { new Guid("75550530-2B20-4496-9AA3-54BF60C63B61"), new DateTime(2020, 2, 27), new Guid("00000000-0000-0000-0000-000000000000"), "Error", "An error occurred while attempting to update the document.", 0 }
                   },
                   schema: "G2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_AzureIndexStatuses_AzureIndexStatusId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropTable(
                name: "AzureIndexStatuses",
                schema: "G2");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_AzureIndexStatusId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AzureIndexStatusId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AzureSearchIndexInfo",
                schema: "G2",
                table: "Profiles");
        }
    }
}
