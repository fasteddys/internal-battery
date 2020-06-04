using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class industrycleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- correct this industry whose value has been overwritten in dev/staging as part of the api tests
UPDATE dbo.Industry SET [Name] = 'Community and Social Service', ModifyDate = GETUTCDATE() WHERE IndustryGuid = 'EB6FC5CE-3EE7-413F-8C02-3E6EB18B6FC7'

-- remove values in these fields which are meaningless
UPDATE dbo.Industry SET ModifyGuid = '00000000-0000-0000-0000-000000000000', CreateGuid = '00000000-0000-0000-0000-000000000000'

-- rename 'Business and Financial Operations' to 'Financial Services'
UPDATE dbo.Industry SET [Name] = 'Financial Services', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = '0D8CD2C6-9B14-4B47-8FA4-36E3F102FBDE'

-- rename 'Healthcare Practitioners and Technical' to 'Healthcare'
UPDATE dbo.Industry SET [Name] = 'Healthcare', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = '2791633A-6D34-4F41-849B-380FDBD60667'

-- logically delete the redundant Healthcare industry
UPDATE dbo.Industry SET IsDeleted = 1, ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = 'F34B8A53-363C-4140-B0DD-464BBBE16E95'

-- update all references to the logically deleted Healthcare industry to use the one that has been renamed (job posting & company)
UPDATE j
SET j.IndustryId = (SELECT TOP 1 IndustryId FROM dbo.Industry WHERE IndustryGuid = '2791633A-6D34-4F41-849B-380FDBD60667')
	, j.ModifyDate = GETUTCDATE()
	, j.ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM dbo.Industry i
INNER JOIN JobPosting j ON i.IndustryId = j.IndustryId
WHERE i.IndustryGuid = 'F34B8A53-363C-4140-B0DD-464BBBE16E95'

UPDATE c
SET c.IndustryId = (SELECT TOP 1 IndustryId FROM dbo.Industry WHERE IndustryGuid = '2791633A-6D34-4F41-849B-380FDBD60667')
	, c.ModifyDate = GETUTCDATE()
	, c.ModifyGuid = '00000000-0000-0000-0000-000000000000'
FROM dbo.Industry i
INNER JOIN Company c ON i.IndustryId = c.IndustryId
WHERE i.IndustryGuid = 'F34B8A53-363C-4140-B0DD-464BBBE16E95'

-- renamed 'Life, Physical, and Social Science' to 'Life Sciences'
UPDATE dbo.Industry SET [Name] = 'Life Sciences', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = '23A9CA57-E4D1-45EB-BD00-6B9C20CF9332'

-- create 'Government' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', 'BD0C7ED9-1417-4CFD-B993-DD2F0839FB6F', 'Government')

-- create 'Energy' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '4EF4D32C-5B74-4B4E-B65B-F31A4DFA639C', 'Energy')

-- create 'Retail' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '6D08C910-F4BE-44B3-86D7-FE218FC3C93F', 'Retail')

-- rename 'Food Services' to 'Hospitality'
UPDATE dbo.Industry SET [Name] = 'Hospitality', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = '7387F948-D89C-4468-8B4E-05BEC72304D8'

-- create 'Telecommunications' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', 'C657E44B-970C-43AB-B6BA-ED5DA650174E', 'Telecommunications')

-- rename 'Technology' to 'Information Technology'
UPDATE dbo.Industry SET [Name] = 'Information Technology', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = '6CAD5EE1-D82B-48AB-A7A3-CBDABFC9B7F0'

-- rename 'Transportation and Moving' to 'Transportation'
UPDATE dbo.Industry SET [Name] = 'Transportation', ModifyDate = GETUTCDATE(), ModifyGuid = '00000000-0000-0000-0000-000000000000' WHERE IndustryGuid = 'C2D623D9-A814-4DDB-998F-4E91DEC598C5'

-- create 'Automotive' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '1D292440-06BD-42C9-9047-6164230F81DE', 'Automotive')

-- create 'Other' industry
INSERT INTO dbo.Industry (IsDeleted, CreateDate, CreateGuid, IndustryGuid, [Name]) VALUES (0, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '8386D172-D5BE-4CE4-852F-5AB3249B9503', 'Other')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
