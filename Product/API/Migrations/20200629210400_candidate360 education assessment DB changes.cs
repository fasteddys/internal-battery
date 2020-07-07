using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class candidate360educationassessmentDBchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "RelevantYear",
                table: "SubscriberEducationHistory",
                type: "SmallInt",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "EducationalDegreeType",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EducationalDegreeTypeCategory",
                columns: table => new
                {
                    EducationalDegreeTypeCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationalDegreeTypeCategoryGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalDegreeTypeCategory", x => x.EducationalDegreeTypeCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TrainingType",
                columns: table => new
                {
                    TrainingTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TrainingTypeGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingType", x => x.TrainingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberTraining",
                columns: table => new
                {
                    SubscriberTrainingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberTrainingGuid = table.Column<Guid>(nullable: false),
                    TrainingTypeId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    TrainingInstitution = table.Column<string>(type: "Varchar(150)", maxLength: 150, nullable: true),
                    TrainingName = table.Column<string>(type: "Varchar(150)", maxLength: 150, nullable: true),
                    RelevantYear = table.Column<short>(type: "SmallInt", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberTraining", x => x.SubscriberTrainingId);
                    table.ForeignKey(
                        name: "FK_SubscriberTraining_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberTraining_TrainingType_TrainingTypeId",
                        column: x => x.TrainingTypeId,
                        principalTable: "TrainingType",
                        principalColumn: "TrainingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalDegreeType_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                column: "EducationalDegreeTypeCategoryId");

            migrationBuilder.CreateIndex(
                name: "UIX_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryGuid",
                table: "EducationalDegreeTypeCategory",
                column: "EducationalDegreeTypeCategoryGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberTraining_SubscriberId",
                table: "SubscriberTraining",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "UIX_SubscriberTraining_SubscriberTrainingGuid",
                table: "SubscriberTraining",
                column: "SubscriberTrainingGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberTraining_TrainingTypeId",
                table: "SubscriberTraining",
                column: "TrainingTypeId");

            migrationBuilder.CreateIndex(
                name: "UIX_TrainingType_TrainingTypeGuid",
                table: "TrainingType",
                column: "TrainingTypeGuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalDegreeType_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType",
                column: "EducationalDegreeTypeCategoryId",
                principalTable: "EducationalDegreeTypeCategory",
                principalColumn: "EducationalDegreeTypeCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"INSERT INTO [dbo].[EducationalDegreeTypeCategory] 
                ([IsDeleted],[CreateDate],[ModifyDate],[CreateGuid],[ModifyGuid],[EducationalDegreeTypeCategoryGuid],[Name],[Sequence])
                VALUES(0,GetUtcDate(),null,'00000000-0000-0000-0000-000000000000',null,'AB88F587-A02D-44DC-9668-FAC204CD3C8F','Secondary Education',1),
                    (0,GetUtcDate(),null,'00000000-0000-0000-0000-000000000000',null,'BA1E68C0-8606-4C76-8E6E-07FBACE89EA9','Higher Education',2), 
                    (0,GetUtcDate(),null,'00000000-0000-0000-0000-000000000000',null,'1BB6E6B4-C784-49B5-8365-7E22811FB4DB','Graduate',3)");

            migrationBuilder.Sql(@"INSERT INTO [dbo].[TrainingType]
                ([IsDeleted],[CreateDate],[ModifyDate],[CreateGuid],[ModifyGuid],[TrainingTypeGuid],[Name],[Sequence]) 
                VALUES(0, GetUtcDate(), null, '00000000-0000-0000-0000-000000000000', null, 'E2CFECE9-BE36-4859-BB9A-93C972B6E18F', 'Technical Training', 1),
	                  (0, GetUtcDate(), null, '00000000-0000-0000-0000-000000000000', null, '61708900-7419-42E0-92DA-A39668F4CF26', 'Professional Training', 2)");

            migrationBuilder.Sql(@"Declare @EducationalDegreeTypeCategoryId int 
                    SELECT @EducationalDegreeTypeCategoryId = [EducationalDegreeTypeCategoryId]
                    FROM[dbo].[EducationalDegreeTypeCategory] 
                    WHERE[Name] = 'Secondary Education' 
                    INSERT INTO[dbo].[EducationalDegreeType] 
                    ([IsDeleted],[CreateDate],[CreateGuid],[EducationalDegreeTypeGuid],[DegreeType],[EducationalDegreeTypeCategoryId],[IsVerified],[Sequence]) 
                    VALUES 
                      (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', 'F47FA511-D40E-4334-8D36-41022513AE5C', 'Currently Enrolled in High School', @EducationalDegreeTypeCategoryId, 1, 1),
                      (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '01FB224E-B0FD-4447-B66E-4F6692BC994C', 'High School Diploma', @EducationalDegreeTypeCategoryId, 1, 2),
                      (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', 'F5FB7179-405F-4DDB-B2E2-4B8E09288769', 'GED', @EducationalDegreeTypeCategoryId, 1, 3),
                      (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '296DCBF9-7306-486B-90F5-FC6EBF620F47', 'Other', @EducationalDegreeTypeCategoryId, 1, 4)"
                    );

            migrationBuilder.Sql(@"Declare @EducationalDegreeTypeCategoryId int

                SELECT @EducationalDegreeTypeCategoryId = [EducationalDegreeTypeCategoryId]
                FROM [dbo].[EducationalDegreeTypeCategory]
                WHERE [Name] = 'Higher Education'

                INSERT INTO [dbo].[EducationalDegreeType]
                ([IsDeleted],[CreateDate],[CreateGuid],[EducationalDegreeTypeGuid],[DegreeType],[EducationalDegreeTypeCategoryId],[IsVerified],[Sequence])
                VALUES  
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', 'BF09D18C-7685-4AAD-B248-3E5BFF340A2C', 'Currently Enrolled in College',@EducationalDegreeTypeCategoryId, 1, 5),
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '992AC43A-E5A9-4E99-89B3-BCECEF30C1C1', 'College Graduate',@EducationalDegreeTypeCategoryId, 1, 6),
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '594677B4-06F7-4DE2-81F0-B94D54DA821F', 'Associates Degree',@EducationalDegreeTypeCategoryId, 1, 7),
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', 'DE11FA1E-53E5-4775-BA66-AD962E6B35B9', 'Completed Partial Coursework',@EducationalDegreeTypeCategoryId, 1, 8)
                ");

            migrationBuilder.Sql(@"Declare @EducationalDegreeTypeCategoryId int

                SELECT @EducationalDegreeTypeCategoryId = [EducationalDegreeTypeCategoryId]
                FROM [dbo].[EducationalDegreeTypeCategory]
                WHERE [Name] = 'Graduate'

                INSERT INTO [dbo].[EducationalDegreeType]
                ([IsDeleted],[CreateDate],[CreateGuid],[EducationalDegreeTypeGuid],[DegreeType],[EducationalDegreeTypeCategoryId],[IsVerified],[Sequence])
                VALUES  
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '7F36713A-1EB7-41D0-AFCE-28FEAA2AD196', 'Currently Enrolled in Graduate Program',@EducationalDegreeTypeCategoryId, 1, 9),
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', '2DDAC994-BFA5-4213-A322-8E382CC8BABF', 'Master''s Degree Graduation',@EducationalDegreeTypeCategoryId, 1, 10),
                  (0, GetUtcDate(), '00000000-0000-0000-0000-000000000000', 'AA4B5B9D-2F1D-4FD5-BEDB-8297CC2127EA', 'Completed Partial Coursework',@EducationalDegreeTypeCategoryId, 1, 11)
                ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalDegreeType_EducationalDegreeTypeCategory_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropTable(
                name: "EducationalDegreeTypeCategory");

            migrationBuilder.DropTable(
                name: "SubscriberTraining");

            migrationBuilder.DropTable(
                name: "TrainingType");

            migrationBuilder.DropIndex(
                name: "IX_EducationalDegreeType_EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "RelevantYear",
                table: "SubscriberEducationHistory");

            migrationBuilder.DropColumn(
                name: "EducationalDegreeTypeCategoryId",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "EducationalDegreeType");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "EducationalDegreeType");

        }
    }
}
