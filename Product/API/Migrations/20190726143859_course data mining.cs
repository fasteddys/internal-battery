using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class coursedatamining : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "Course",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "UnsubscribeGroupId",
                table: "CampaignPartner",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "CoursePageStatus",
                columns: table => new
                {
                    CoursePageStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CoursePageStatusGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePageStatus", x => x.CoursePageStatusId);
                });

            migrationBuilder.CreateTable(
                name: "CourseSite",
                columns: table => new
                {
                    CourseSiteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseSiteGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Uri = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSite", x => x.CourseSiteId);
                });

            migrationBuilder.CreateTable(
                name: "CoursePage",
                columns: table => new
                {
                    CoursePageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CoursePageGuid = table.Column<Guid>(nullable: false),
                    Uri = table.Column<string>(nullable: false),
                    CoursePageStatusId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: true),
                    CourseSiteId = table.Column<int>(nullable: false),
                    UniqueIdentifier = table.Column<string>(nullable: false),
                    RawData = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePage", x => x.CoursePageId);
                    table.ForeignKey(
                        name: "FK_CoursePage_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePage_CoursePageStatus_CoursePageStatusId",
                        column: x => x.CoursePageStatusId,
                        principalTable: "CoursePageStatus",
                        principalColumn: "CoursePageStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursePage_CourseSite_CourseSiteId",
                        column: x => x.CourseSiteId,
                        principalTable: "CourseSite",
                        principalColumn: "CourseSiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursePage_CourseId",
                table: "CoursePage",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePage_CoursePageStatusId",
                table: "CoursePage",
                column: "CoursePageStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePage_CourseSiteId",
                table: "CoursePage",
                column: "CourseSiteId");

            migrationBuilder.CreateIndex(
                name: "UIX_CoursePage_CourseSite_UniqueIdentifier",
                table: "CoursePage",
                columns: new[] { "UniqueIdentifier", "CourseSiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UIX_CourseSite_Name",
                table: "CourseSite",
                column: "Name",
                unique: true);

            migrationBuilder.InsertData(
                table: "CourseSite",
                columns: new[] { "CourseSiteGuid", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Uri" },
                values: new object[] { new Guid("7962B9AF-1A1E-4F0B-94B1-8CFED5F328A6"), new DateTime(2019, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "ITProTV", "https://www.itpro.tv/sitemap.xml" });


            migrationBuilder.InsertData(
                table: "CoursePageStatus",
                columns: new[] { "CoursePageStatusGuid", "CoursePageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("EF050BB4-51CF-442B-8B4E-6ECB134AA73B"), 1, new DateTime(2019, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Pending", "This status is applied to a CoursePage when raw data has been inserted or modified. This occurs before processing occurs to act on an associated Course." });

            migrationBuilder.InsertData(
                table: "CoursePageStatus",
                columns: new[] { "CoursePageStatusGuid", "CoursePageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("9C1DAA78-D499-4C37-A6DA-EEBE6A03F481"), 2, new DateTime(2019, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Active", "This status is applied to a CoursePage when after a job page has been processed successfully and associated with an active Course." });

            migrationBuilder.InsertData(
                table: "CoursePageStatus",
                columns: new[] { "CoursePageStatusGuid", "CoursePageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("8B26279D-0212-4F0E-9FFA-F77ACE7CC460"), 3, new DateTime(2019, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Error", "This status is applied to a CoursePage when an error occurs during CoursePage parsing which prevents an associated Course record from being inserted or updated." });

            migrationBuilder.InsertData(
                table: "CoursePageStatus",
                columns: new[] { "CoursePageStatusGuid", "CoursePageStatusId", "CreateDate", "CreateGuid", "IsDeleted", "Name", "Description" },
                values: new object[] { new Guid("6DEDA707-E4CF-4B7A-A5C9-8BE349E1F3C7"), 4, new DateTime(2019, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, "Deleted", "This status is applied to a CoursePage when it is discovered that a previously seen Course is no longer visible on a CourseSite." });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoursePage");

            migrationBuilder.DropTable(
                name: "CoursePageStatus");

            migrationBuilder.DropTable(
                name: "CourseSite");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Course");

            migrationBuilder.AlterColumn<int>(
                name: "UnsubscribeGroupId",
                table: "CampaignPartner",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
