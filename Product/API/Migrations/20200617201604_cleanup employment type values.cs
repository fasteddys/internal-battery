using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace UpDiddyApi.Migrations
{
    public partial class cleanupemploymenttypevalues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // logically delete all of the existing employment type values
            migrationBuilder.Sql("UPDATE dbo.EmploymentType SET IsDeleted = 1, ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000'");

            // create the new employment type values
            migrationBuilder.InsertData(
                    table: "EmploymentType",
                    columns: new[] { "EmploymentTypeGuid", "CreateDate", "CreateGuid", "Name", "IsDeleted" },
                    values: new object[,]
                    {
                        { new Guid("D39DA776-F589-4E0A-BCAD-21BE30C6838D"), new DateTime(2020, 6, 17), new Guid("00000000-0000-0000-0000-000000000000"), "Part-Time Contract", 0 },
                        { new Guid("05585BA6-852A-45DD-A546-DD5D1872339D"), new DateTime(2020, 6, 17), new Guid("00000000-0000-0000-0000-000000000000"), "Full-Time Contract", 0 },
                        { new Guid("432AE21A-C4B3-4B35-B475-DB5E3B167DDE"), new DateTime(2020, 6, 17), new Guid("00000000-0000-0000-0000-000000000000"), "Full-Time Employee", 0 },
                        { new Guid("5B0650D7-13F5-4F66-B8B4-18CA8ECE77D2"), new DateTime(2020, 6, 17), new Guid("00000000-0000-0000-0000-000000000000"), "Remote Only", 0 }
                    },
                    schema: "dbo");

            // cleanup references to employment types using mappings provided by Kim
            migrationBuilder.Sql(@"INSERT INTO G2.ProfileEmploymentTypes (IsDeleted, CreateDate, CreateGuid, ProfileEmploymentTypeGuid, ProfileId, EmploymentTypeId)
SELECT 0, '2020-06-17', '00000000-0000-0000-0000-000000000000', NEWID(), uniqueProfiles.ProfileId, (SELECT TOP 1 EmploymentTypeId FROM dbo.EmploymentType WHERE [Name] = 'Full-Time Contract')
FROM (
	SELECT DISTINCT pet.ProfileId
	FROM G2.ProfileEmploymentTypes pet
	INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
	WHERE et.[Name] IN ('Temporary', 'Contractor', 'Full-Time')
	AND NOT EXISTS(
		SELECT *
		FROM G2.ProfileEmploymentTypes e_pet
		INNER JOIN dbo.EmploymentType e_et ON e_pet.EmploymentTypeId = e_et.EmploymentTypeId
		AND e_et.[Name] = 'Full-Time Contract')
	) uniqueProfiles");

            migrationBuilder.Sql(@"INSERT INTO G2.ProfileEmploymentTypes (IsDeleted, CreateDate, CreateGuid, ProfileEmploymentTypeGuid, ProfileId, EmploymentTypeId)
SELECT 0, '2020-06-17', '00000000-0000-0000-0000-000000000000', NEWID(), uniqueProfiles.ProfileId, (SELECT TOP 1 EmploymentTypeId FROM dbo.EmploymentType WHERE [Name] = 'Full-Time Employee')
FROM (
	SELECT DISTINCT pet.ProfileId
	FROM G2.ProfileEmploymentTypes pet
	INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
	WHERE et.[Name] IN ('Full-Time')
	AND NOT EXISTS(
		SELECT *
		FROM G2.ProfileEmploymentTypes e_pet
		INNER JOIN dbo.EmploymentType e_et ON e_pet.EmploymentTypeId = e_et.EmploymentTypeId
		AND e_et.[Name] = 'Full-Time Employee')
	) uniqueProfiles");

            migrationBuilder.Sql(@"UPDATE 
	pet
SET
	pet.IsDeleted = 1
	, pet.ModifyDate = '2020-06-17'
	, pet.ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM 
	g2.ProfileEmploymentTypes pet
	INNER JOIN dbo.EmploymentType et ON pet.EmploymentTypeId = et.EmploymentTypeId
	WHERE et.IsDeleted = 1");

            // we can get rid of all references to employment type for job postings since they are not used by the job search
            migrationBuilder.Sql("UPDATE dbo.JobPosting SET EmploymentTypeId = NULL, ModifyDate = '2020-06-17', ModifyGuid = '00000000-0000-0000-0000-000000000000'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
