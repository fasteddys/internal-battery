using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class b2bhiringmanagerpipelines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "B2B");
            
            migrationBuilder.CreateTable(
                name: "Pipelines",
                schema: "B2B",
                columns: table => new
                {
                    PipelineId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PipelineGuid = table.Column<Guid>(nullable: false),
                    HiringManagerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 25, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pipelines", x => x.PipelineId);
                    table.ForeignKey(
                        name: "FK_Pipelines_HiringManagers_HiringManagerId",
                        column: x => x.HiringManagerId,
                        principalSchema: "B2B",
                        principalTable: "HiringManagers",
                        principalColumn: "HiringManagerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineProfiles",
                schema: "B2B",
                columns: table => new
                {
                    PipelineProfileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PipelineProfileGuid = table.Column<Guid>(nullable: false),
                    PipelineId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineProfiles", x => x.PipelineProfileId);
                    table.ForeignKey(
                        name: "FK_PipelineProfiles_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "B2B",
                        principalTable: "Pipelines",
                        principalColumn: "PipelineId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PipelineProfiles_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "G2",
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineProfiles_PipelineId",
                schema: "B2B",
                table: "PipelineProfiles",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "UIX_PipelineProfile_Profile_Pipeline",
                schema: "B2B",
                table: "PipelineProfiles",
                columns: new[] { "ProfileId", "PipelineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_Pipeline_HiringManager_Name_IsDeleted",
                schema: "B2B",
                table: "Pipelines",
                columns: new[] { "HiringManagerId", "Name", "IsDeleted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineProfiles",
                schema: "B2B");

            migrationBuilder.DropTable(
                name: "Pipelines",
                schema: "B2B");
        }
    }
}
