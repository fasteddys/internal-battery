using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class g2profile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "G2");

            migrationBuilder.CreateTable(
                name: "ContactTypes",
                schema: "G2",
                columns: table => new
                {
                    ContactTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ContactTypeGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTypes", x => x.ContactTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ProfileDocuments",
                schema: "G2",
                columns: table => new
                {
                    ProfileDocumentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileDocumentGuid = table.Column<Guid>(nullable: false),
                    BlobStorageUrl = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileDocuments", x => x.ProfileDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "G2",
                columns: table => new
                {
                    ProfileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileGuid = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 100, nullable: true),
                    LastName = table.Column<string>(maxLength: 100, nullable: true),
                    Email = table.Column<string>(maxLength: 254, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    ContactTypeId = table.Column<int>(nullable: true),
                    StreetAddress = table.Column<string>(maxLength: 100, nullable: true),
                    CityId = table.Column<int>(nullable: true),
                    StateId = table.Column<int>(nullable: false),
                    PostalId = table.Column<int>(nullable: true),
                    ExperienceLevelId = table.Column<int>(nullable: true),
                    EmploymentTypeId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(maxLength: 100, nullable: true),
                    IsWillingToTravel = table.Column<bool>(nullable: true),
                    IsActiveJobSeeker = table.Column<bool>(nullable: true),
                    IsCurrentlyEmployed = table.Column<bool>(nullable: true),
                    IsWillingToWorkProBono = table.Column<bool>(nullable: true),
                    CurrentRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DesiredRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Goals = table.Column<string>(maxLength: 500, nullable: true),
                    Preferences = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_Profiles_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Profiles_ContactTypes_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalSchema: "G2",
                        principalTable: "ContactTypes",
                        principalColumn: "ContactTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalTable: "EmploymentType",
                        principalColumn: "EmploymentTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_ExperienceLevel_ExperienceLevelId",
                        column: x => x.ExperienceLevelId,
                        principalTable: "ExperienceLevel",
                        principalColumn: "ExperienceLevelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_Postal_PostalId",
                        column: x => x.PostalId,
                        principalTable: "Postal",
                        principalColumn: "PostalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Profiles_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileComments",
                schema: "G2",
                columns: table => new
                {
                    ProfileCommentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileCommentGuid = table.Column<Guid>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 500, nullable: true),
                    IsVisibleToCompany = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileComments", x => x.ProfileCommentId);
                    table.ForeignKey(
                        name: "FK_ProfileComments_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileSearchLocations",
                schema: "G2",
                columns: table => new
                {
                    ProfileSearchLocationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileSearchLocationGuid = table.Column<Guid>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: false),
                    PostalId = table.Column<int>(nullable: true),
                    SearchRadius = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileSearchLocations", x => x.ProfileSearchLocationId);
                    table.ForeignKey(
                        name: "FK_ProfileSearchLocations_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileSearchLocations_Postal_PostalId",
                        column: x => x.PostalId,
                        principalTable: "Postal",
                        principalColumn: "PostalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileSearchLocations_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileSkills",
                schema: "G2",
                columns: table => new
                {
                    ProfileSkillId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProfileSkillGuid = table.Column<Guid>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    SkillId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileSkills", x => x.ProfileSkillId);
                    table.ForeignKey(
                        name: "FK_ProfileSkills_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileSkills_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UIX_ContactType_Name",
                schema: "G2",
                table: "ContactTypes",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileComments_ProfileId",
                schema: "G2",
                table: "ProfileComments",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_CityId",
                schema: "G2",
                table: "Profiles",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_CompanyId",
                schema: "G2",
                table: "Profiles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ContactTypeId",
                schema: "G2",
                table: "Profiles",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_EmploymentTypeId",
                schema: "G2",
                table: "Profiles",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ExperienceLevelId",
                schema: "G2",
                table: "Profiles",
                column: "ExperienceLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_PostalId",
                schema: "G2",
                table: "Profiles",
                column: "PostalId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_StateId",
                schema: "G2",
                table: "Profiles",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "UIX_Profile_Subscriber_Company",
                schema: "G2",
                table: "Profiles",
                columns: new[] { "SubscriberId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSearchLocations_CityId",
                schema: "G2",
                table: "ProfileSearchLocations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSearchLocations_PostalId",
                schema: "G2",
                table: "ProfileSearchLocations",
                column: "PostalId");

            migrationBuilder.CreateIndex(
                name: "UIX_ProfileSearchLocation_Profile_City_Postal",
                schema: "G2",
                table: "ProfileSearchLocations",
                columns: new[] { "ProfileId", "CityId", "PostalId" },
                unique: true,
                filter: "[PostalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSkills_SkillId",
                schema: "G2",
                table: "ProfileSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "UIX_ProfileSkill_Profile_Skill",
                schema: "G2",
                table: "ProfileSkills",
                columns: new[] { "ProfileId", "SkillId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileComments",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ProfileDocuments",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ProfileSearchLocations",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ProfileSkills",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "G2");

            migrationBuilder.DropTable(
                name: "ContactTypes",
                schema: "G2");
        }
    }
}
