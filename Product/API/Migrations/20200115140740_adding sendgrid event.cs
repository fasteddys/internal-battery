using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingsendgridevent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SendGridEvent",
                columns: table => new
                {
                    SendGridEventId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SendGridEventGuid = table.Column<Guid>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Timestamp = table.Column<long>(nullable: false),
                    Smtp_id = table.Column<string>(nullable: true),
                    Event = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    Sg_event_id = table.Column<string>(nullable: true),
                    Sg_message_id = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Attempt = table.Column<string>(nullable: true),
                    UserAgent = table.Column<string>(nullable: true),
                    Ip = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Tls = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendGridEvent", x => x.SendGridEventId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SendGridEvent");
        }
    }
}
