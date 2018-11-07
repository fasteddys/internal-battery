using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class coursevariantnormalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {      
            migrationBuilder.AddColumn<int>(
                name: "CourseVariantId",
                table: "PromoCodeRedemption",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CourseVariant",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "CourseVariant",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE cv SET cv.CourseId = c.CourseId FROM dbo.CourseVariant cv INNER JOIN dbo.Course c ON cv.CourseGuid = c.CourseGuid", false);
            
            migrationBuilder.AddColumn<int>(
                name: "CourseVariantTypeId",
                table: "CourseVariant",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.CreateTable(
                name: "CourseVariantType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseVariantGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariantType", x => x.CourseVariantTypeId);
                });

            migrationBuilder.InsertData(
                table: "CourseVariantType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CourseVariantGuid", "Name"},
                values: new object[] { 0, DateTime.MinValue, null, Guid.NewGuid(), null, Guid.NewGuid(), "Self-Paced" });

            migrationBuilder.InsertData(
                table: "CourseVariantType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "CourseVariantGuid", "Name" },
                values: new object[] { 0, DateTime.MinValue, null, Guid.NewGuid(), null, Guid.NewGuid(), "Instructor-Led" });

            migrationBuilder.Sql("UPDATE cv SET cv.CourseVariantTypeId = cvt.CourseVariantTypeId FROM dbo.CourseVariantType cvt INNER JOIN dbo.CourseVariant cv ON (CASE WHEN cvt.Name = 'Instructor-Led' THEN 'instructor' WHEN cvt.Name = 'Self-Paced' THEN 'selfpaced' ELSE NULL END) = cv.VariantType", false);
            

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_CourseVariantId",
                table: "PromoCodeRedemption",
                column: "CourseVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVariant_CourseId",
                table: "CourseVariant",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVariant_CourseVariantTypeId",
                table: "CourseVariant",
                column: "CourseVariantTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVariant_Course_CourseId",
                table: "CourseVariant",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVariant_CourseVariantType_CourseVariantTypeId",
                table: "CourseVariant",
                column: "CourseVariantTypeId",
                principalTable: "CourseVariantType",
                principalColumn: "CourseVariantTypeId",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.Sql("UPDATE pcr SET pcr.CourseVariantId = cv.CourseVariantId FROM dbo.PromoCodeRedemption pcr INNER JOIN dbo.Course c ON pcr.CourseId = c.CourseId INNER JOIN dbo.CourseVariant cv ON c.CourseGuid = cv.CourseGuid AND cv.VariantType = 'selfpaced'", false);
            
            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodeRedemption_CourseVariant_CourseVariantId",
                table: "PromoCodeRedemption",
                column: "CourseVariantId",
                principalTable: "CourseVariant",
                principalColumn: "CourseVariantId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropColumn(
                name: "CourseGuid",
                table: "CourseVariant");

            migrationBuilder.DropColumn(
                name: "VariantType",
                table: "CourseVariant");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Course");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Course",
                nullable: true);

            migrationBuilder.Sql("UPDATE dbo.Course SET [Price] = 249.00", false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CourseVariant",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AddColumn<string>(
                name: "VariantType",
                table: "CourseVariant",
                nullable: true);

            migrationBuilder.Sql("UPDATE cv SET cv.VariantType = CASE WHEN cvt.Name = 'Self-Paced' THEN 'selfpaced' WHEN cvt.Name = 'Instructor-Led' THEN 'instructor' ELSE NULL END FROM CourseVariant cv INNER JOIN CourseVariantType cvt ON cv.CourseVariantTypeId = cvt.CourseVariantTypeId", false);
            
            migrationBuilder.AddColumn<Guid>(
                name: "CourseGuid",
                table: "CourseVariant",
                nullable: true);
            migrationBuilder.Sql("UPDATE cv SET cv.CourseGuid = c.CourseGuid FROM CourseVariant cv INNER JOIN Course c on cv.CourseId = c.CourseId", false);

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVariant_Course_CourseId",
                table: "CourseVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseVariant_CourseVariantType_CourseVariantTypeId",
                table: "CourseVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodeRedemption_CourseVariant_CourseVariantId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropTable(
                name: "CourseVariantType");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodeRedemption_CourseVariantId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropIndex(
                name: "IX_CourseVariant_CourseId",
                table: "CourseVariant");

            migrationBuilder.DropIndex(
                name: "IX_CourseVariant_CourseVariantTypeId",
                table: "CourseVariant");

            migrationBuilder.DropColumn(
                name: "CourseVariantId",
                table: "PromoCodeRedemption");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CourseVariant");

            migrationBuilder.DropColumn(
                name: "CourseVariantTypeId",
                table: "CourseVariant");
        }
    }
}
