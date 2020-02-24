using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class g2updates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                    table: "ContactTypes",
                    columns: new[] { "ContactTypeGuid", "CreateDate", "CreateGuid", "Name", "Description", "IsDeleted", "Sequence" },
                    values: new object[,]
                    {
                              { new Guid("EF72FEAD-C895-43BF-94C4-330354C1BE9A"), new DateTime(2020, 2, 24), new Guid("00000000-0000-0000-0000-000000000000"), "Phone", "The user prefers to be contacted by phone.", 0, 1 },
                              { new Guid("1BCA9958-BE24-485F-B7E5-2EFA40942824"), new DateTime(2020, 2, 24), new Guid("00000000-0000-0000-0000-000000000000"), "Text", "The user prefers to be contacted by text message.", 0, 2 },
                              { new Guid("1C69165E-8005-4E62-8E70-E4EC4EAFCEFB"), new DateTime(2020, 2, 24), new Guid("00000000-0000-0000-0000-000000000000"), "Email", "The user prefers to be contacted by email.", 0, 3 },
                              { new Guid("65890806-DD8B-4E49-8A92-619352D09389"), new DateTime(2020, 2, 24), new Guid("00000000-0000-0000-0000-000000000000"), "DNC", "The user does not wish to be contacted.", 0, 4 }
                    },
                    schema: "G2");

            migrationBuilder.InsertData(
                   table: "Company",
                   columns: new[] { "CompanyGuid", "CreateDate", "CreateGuid", "CompanyName", "IsJobPoster", "IsDeleted", "IsHiringAgency" },
                   values: new object[,]
                   {
                        { new Guid("25166261-ABA6-43A4-8084-57EEB2D6FF3E"), new DateTime(2020, 2, 24), new Guid("00000000-0000-0000-0000-000000000000"), "CareerCircle", 1, 0, 1 }
                   });

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_State_StateId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                schema: "G2",
                table: "Profiles",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                schema: "G2",
                table: "ProfileDocuments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProfileTags",
                schema: "G2",
                columns: table => new
                {
                    ProfileTagId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileTagGuid = table.Column<Guid>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileTags", x => x.ProfileTagId);
                    table.ForeignKey(
                        name: "FK_ProfileTags_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileTags_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wishlists",
                schema: "G2",
                columns: table => new
                {
                    WishlistId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    WishlistGuid = table.Column<Guid>(nullable: false),
                    RecruiterId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 25, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.WishlistId);
                    table.ForeignKey(
                        name: "FK_Wishlists_Recruiter_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiter",
                        principalColumn: "RecruiterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileWishlists",
                schema: "G2",
                columns: table => new
                {
                    ProfileWishlistId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileWishlistGuid = table.Column<Guid>(nullable: false),
                    WishlistId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileWishlists", x => x.ProfileWishlistId);
                    table.ForeignKey(
                        name: "FK_ProfileWishlists_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileWishlists_Wishlists_WishlistId",
                        column: x => x.WishlistId,
                        principalSchema: "G2",
                        principalTable: "Wishlists",
                        principalColumn: "WishlistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileDocuments_ProfileId",
                schema: "G2",
                table: "ProfileDocuments",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTags_ProfileId",
                schema: "G2",
                table: "ProfileTags",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTags_TagId",
                schema: "G2",
                table: "ProfileTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileWishlists_ProfileId",
                schema: "G2",
                table: "ProfileWishlists",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileWishlists_WishlistId",
                schema: "G2",
                table: "ProfileWishlists",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_RecruiterId",
                schema: "G2",
                table: "Wishlists",
                column: "RecruiterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileDocuments_Profiles_ProfileId",
                schema: "G2",
                table: "ProfileDocuments",
                column: "ProfileId",
                principalSchema: "G2",
                principalTable: "Profiles",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_State_StateId",
                schema: "G2",
                table: "Profiles",
                column: "StateId",
                principalTable: "State",
                principalColumn: "StateId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileDocuments_Profiles_ProfileId",
                schema: "G2",
                table: "ProfileDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_State_StateId",
                schema: "G2",
                table: "Profiles");

            migrationBuilder.DropTable(
                name: "ProfileTags",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ProfileWishlists",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "Wishlists",
                schema: "G2");

            migrationBuilder.DropIndex(
                name: "IX_ProfileDocuments_ProfileId",
                schema: "G2",
                table: "ProfileDocuments");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                schema: "G2",
                table: "ProfileDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                schema: "G2",
                table: "Profiles",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_State_StateId",
                schema: "G2",
                table: "Profiles",
                column: "StateId",
                principalTable: "State",
                principalColumn: "StateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
