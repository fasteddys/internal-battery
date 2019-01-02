using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class AddEducationTypeData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 
            migrationBuilder.InsertData(
                table: "EducationalDegreeType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Some High School" }
                );

            migrationBuilder.InsertData(
                table: "EducationalDegreeType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "High School Diploma / GED" }
                );

            migrationBuilder.InsertData(
               table: "EducationalDegreeType",
               columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
               values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Some College" }
                );

            migrationBuilder.InsertData(
                table: "EducationalDegreeType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Associate's Degree" }
                );

            migrationBuilder.InsertData(
               table: "EducationalDegreeType",
               columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Bachelor's Degree" }
                );

            migrationBuilder.InsertData(
                table: "EducationalDegreeType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Master's Degree"}
                );

            migrationBuilder.InsertData(
                table: "EducationalDegreeType",
                columns: new[] { "IsDeleted", "CreateDate", "ModifyDate", "CreateGuid", "ModifyGuid", "EducationalDegreeTypeGuid", "DegreeType" },
                values: new object[] { 0, DateTime.Now, DateTime.Now, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Doctorate Degree" }
                );

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [EducationalDegreeType]", true);
        }

    }
}
