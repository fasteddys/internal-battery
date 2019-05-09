using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class decoupleRecruiterfromSubscriber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from JobPosting");
            migrationBuilder.Sql("delete from RecruiterCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Subscriber_SubscriberId",
                table: "JobPosting");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Subscriber_SubscriberId",
                table: "RecruiterCompany");

            migrationBuilder.DropColumn(
                name: "CompanyGuid",
                table: "RecruiterCompany");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "RecruiterCompany",
                newName: "RecruiterId");

            migrationBuilder.RenameIndex(
                name: "IX_RecruiterCompany_SubscriberId",
                table: "RecruiterCompany",
                newName: "IX_RecruiterCompany_RecruiterId");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "JobPosting",
                newName: "RecruiterId");

            migrationBuilder.RenameIndex(
                name: "IX_JobPosting_SubscriberId",
                table: "JobPosting",
                newName: "IX_JobPosting_RecruiterId");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "RecruiterCompany",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Recruiter",
                columns: table => new
                {
                    RecruiterId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    RecruiterGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiter", x => x.RecruiterId);
                    table.ForeignKey(
                        name: "FK_Recruiter_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecruiterAction",
                columns: table => new
                {
                    RecruiterActionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    RecruiterActionGuid = table.Column<Guid>(nullable: false),
                    RecruiterId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false),
                    OccurredDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EntityId = table.Column<int>(nullable: true),
                    EntityTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterAction", x => x.RecruiterActionId);
                    table.ForeignKey(
                        name: "FK_RecruiterAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecruiterAction_EntityType_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "EntityType",
                        principalColumn: "EntityTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruiterAction_Recruiter_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiter",
                        principalColumn: "RecruiterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiter_SubscriberId",
                table: "Recruiter",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAction_ActionId",
                table: "RecruiterAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAction_EntityTypeId",
                table: "RecruiterAction",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAction_RecruiterId",
                table: "RecruiterAction",
                column: "RecruiterId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Recruiter_RecruiterId",
                table: "JobPosting",
                column: "RecruiterId",
                principalTable: "Recruiter",
                principalColumn: "RecruiterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Recruiter_RecruiterId",
                table: "RecruiterCompany",
                column: "RecruiterId",
                principalTable: "Recruiter",
                principalColumn: "RecruiterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Recruiter_RecruiterId",
                table: "JobPosting");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_RecruiterCompany_Recruiter_RecruiterId",
                table: "RecruiterCompany");

            migrationBuilder.DropTable(
                name: "RecruiterAction");

            migrationBuilder.DropTable(
                name: "Recruiter");
            
            migrationBuilder.RenameColumn(
                name: "RecruiterId",
                table: "RecruiterCompany",
                newName: "SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_RecruiterCompany_RecruiterId",
                table: "RecruiterCompany",
                newName: "IX_RecruiterCompany_SubscriberId");

            migrationBuilder.RenameColumn(
                name: "RecruiterId",
                table: "JobPosting",
                newName: "SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_JobPosting_RecruiterId",
                table: "JobPosting",
                newName: "IX_JobPosting_SubscriberId");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "RecruiterCompany",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyGuid",
                table: "RecruiterCompany",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Subscriber_SubscriberId",
                table: "JobPosting",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Company_CompanyId",
                table: "RecruiterCompany",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecruiterCompany_Subscriber_SubscriberId",
                table: "RecruiterCompany",
                column: "SubscriberId",
                principalTable: "Subscriber",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
