using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class CreateLanguageProficiencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    LanguageGuid = table.Column<Guid>(nullable: false),
                    LanguageName = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageId);
                });

            migrationBuilder.CreateTable(
                name: "ProficiencyLevels",
                columns: table => new
                {
                    ProficiencyLevelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    ProficiencyLevelGuid = table.Column<Guid>(nullable: false),
                    ProficiencyLevelName = table.Column<string>(maxLength: 500, nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProficiencyLevels", x => x.ProficiencyLevelId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberLanguageProficiencies",
                columns: table => new
                {
                    SubscriberLanguageProficiencyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberLanguageProficiencyGuid = table.Column<Guid>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    ProficiencyLevelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberLanguageProficiencies", x => x.SubscriberLanguageProficiencyId);
                    table.ForeignKey(
                        name: "FK_SubscriberLanguageProficiencies_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "LanguageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberLanguageProficiencies_ProficiencyLevels_ProficiencyLevelId",
                        column: x => x.ProficiencyLevelId,
                        principalTable: "ProficiencyLevels",
                        principalColumn: "ProficiencyLevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberLanguageProficiencies_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "LanguageGuid", "CreateDate", "CreateGuid", "LanguageName", "IsDeleted" },
                values: new object[,]
                {
                    { new Guid("C12502AC-0A8F-42EE-A531-F749C877A2D8"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Afrikaans", 0 },
                    { new Guid("59262E70-65CB-4691-B136-77E933D44757"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Albanian", 0 },
                    { new Guid("1C0EC8D9-AD1F-4626-9988-71C3DE8F3C91"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Amharic", 0 },
                    { new Guid("ED1056ED-AD3A-4CCD-8126-B306E4085CF4"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Arabic (Egyptian Spoken)", 0 },
                    { new Guid("DA0E92EC-E48A-4398-858A-73004C780ED6"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Arabic (Levantine)", 0 },
                    { new Guid("4D6C488A-7D12-483E-822E-A2CF9C5AEE8F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Arabic (Modern Standard)", 0 },
                    { new Guid("BD692A3E-E2A6-4B8A-8642-3C91C56FAFF1"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Arabic (Moroccan Spoken)", 0 },
                    { new Guid("C56C48BA-E0DC-46AF-873F-E9508339B6A1"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Arabic (Overview)", 0 },
                    { new Guid("212FB5E5-AFF0-41B3-9592-236B368AB2D0"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Aramaic", 0 },
                    { new Guid("9166A5E9-D256-4480-9CAE-BF0B5D0A4BEB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Armenian", 0 },
                    { new Guid("6EB67E47-AABD-4996-AEB5-4286B95BE75E"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Assamese", 0 },
                    { new Guid("8585F05B-877F-4AEF-B1EF-64C9F10096AE"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Aymara", 0 },
                    { new Guid("25CBA067-41AE-42DE-BD93-5D6FC95B0303"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Azerbaijani", 0 },
                    { new Guid("47966B28-9BFC-44D4-BCE4-E43B15EAD0AF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Balochi", 0 },
                    { new Guid("E4D6CD26-3B8F-4AA6-A080-094BE3517381"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bamanankan", 0 },
                    { new Guid("A2D82548-3042-4496-848F-E95DD118BB0F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bashkort (Bashkir)", 0 },
                    { new Guid("555C47AA-D2F0-4A2B-8EE8-B01FCF0A98AA"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Basque", 0 },
                    { new Guid("0556FDA9-59D4-4218-907C-2C7B92A2CCEF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Belarusan", 0 },
                    { new Guid("55D54BAC-A664-4EC1-87CE-D91BAFCDF9DD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bengali", 0 },
                    { new Guid("DC0E4466-420A-4781-B37A-4424E5FC84FF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bhojpuri", 0 },
                    { new Guid("D00FD4E6-64BD-46B8-95EA-1AA8D5625787"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bislama", 0 },
                    { new Guid("1FE59B64-60A6-41B8-AA7E-D751670BC8FD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bosnian", 0 },
                    { new Guid("66AAEA62-DB10-4BA9-A995-75A0BBFC4BBB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Brahui", 0 },
                    { new Guid("2E07F889-5B15-494E-8A23-7788B06B2EFC"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Bulgarian", 0 },
                    { new Guid("763863D0-0713-472F-B205-7152D03D1385"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Burmese", 0 },
                    { new Guid("B0596735-E0BE-471F-A41D-397E4D205DF6"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Cantonese", 0 },
                    { new Guid("37A45573-4AFE-41DD-9A0A-2EFB71C5C5EC"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Catalan", 0 },
                    { new Guid("94377E75-9A4D-4E38-8A72-3554F992D122"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Cebuano", 0 },
                    { new Guid("1DB73418-8E0B-413C-976D-5597D5E7FAB9"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Chechen", 0 },
                    { new Guid("442093C2-4953-4819-95E6-171FFAEF8923"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Cherokee", 0 },
                    { new Guid("2A042338-1DD7-496E-8867-88208AFA77A0"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Croatian", 0 },
                    { new Guid("8E1FCB39-BF73-42CD-A9F8-8B4DAF802C79"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Czech", 0 },
                    { new Guid("0B8BA693-9A14-4A44-B3AA-4B2CB9751EAD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Dakota", 0 },
                    { new Guid("A84F3106-610F-470E-95A2-87475105B524"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Danish", 0 },
                    { new Guid("14DAA8E1-66C0-4CD9-AB36-0EB7E5C5C067"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Dari", 0 },
                    { new Guid("CB4181E1-3A18-4B31-86E1-89F37E983B88"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Dholuo", 0 },
                    { new Guid("2BE04929-E4E0-4279-8D57-41616CD00C21"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Dutch", 0 },
                    { new Guid("2E3CA8ED-A012-4EB9-9FD6-9C5A7E6D7C47"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "English", 0 },
                    { new Guid("8A364FE8-2245-4932-AE20-A66631494597"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Esperanto", 0 },
                    { new Guid("7B9A1247-53D9-4FED-903B-9B1DE4A2C016"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Estonian", 0 },
                    { new Guid("61E0EA0A-B52B-4F91-A889-2F8706681476"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Éwé", 0 },
                    { new Guid("A957511B-D0F5-4DC3-B9B7-691D4370915E"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Finnish", 0 },
                    { new Guid("259B8C2D-0F98-478D-A626-0C1801EDE54F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "French", 0 },
                    { new Guid("1DF0BDEB-6E10-4473-9E41-1F13C09CEF67"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Georgian", 0 },
                    { new Guid("3119B8E7-4D4B-4C0F-BC64-EFD825ACB206"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "German", 0 },
                    { new Guid("99FAEF8D-A952-409D-9BCB-3A2593952BD9"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Gikuyu", 0 },
                    { new Guid("5E0833D3-70B8-46C7-A9F6-B80B8E518ADB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Greek", 0 },
                    { new Guid("2BC1695F-51F9-4210-9975-F1CFAC1EE255"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Guarani", 0 },
                    { new Guid("8D4FC190-DA2E-4D90-85FB-CD99D54496C3"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Gujarati", 0 },
                    { new Guid("D63DB70A-1FE1-40A3-9B6B-9CCCEA59111F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Haitian Creole", 0 },
                    { new Guid("667A50C6-A325-465A-99F7-2C68F0A73BED"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hausa", 0 },
                    { new Guid("5987E1DE-2BF2-4CCC-8364-6542C73523A0"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hawaiian", 0 },
                    { new Guid("084FE8F5-E323-402A-A77E-450ED579C745"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hawaiian Creole", 0 },
                    { new Guid("BED99CEF-FC15-41B9-A65A-66C6467A9932"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hebrew", 0 },
                    { new Guid("0FF995C4-5901-40CE-8E31-F4A072EC7FDB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hiligaynon", 0 },
                    { new Guid("508DB78F-D9B3-420D-8526-48B90D656496"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hindi", 0 },
                    { new Guid("4BFA06F2-5514-4800-BACC-6D8056ECE8F7"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Hungarian", 0 },
                    { new Guid("ED5D9577-619F-46A7-A69E-BD73D17E28C2"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Icelandic", 0 },
                    { new Guid("54B2A1A2-8C5B-41F0-967A-555F008247AC"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Igbo", 0 },
                    { new Guid("BB94B752-9C7E-41F6-A9C0-C7DD4CD022D8"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Ilocano", 0 },
                    { new Guid("B093C9AF-9672-4C76-B556-0DCCEFA68EF6"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Indonesian (Bahasa Indonesia)", 0 },
                    { new Guid("9086B178-A377-460E-ABB4-E1168E3AFA12"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Inuit/Inupiaq", 0 },
                    { new Guid("16578AFC-B679-4B10-88A2-39E91034B25F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Irish Gaelic", 0 },
                    { new Guid("69DF21F1-D8C3-444A-82E9-9080A67C4186"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Italian", 0 },
                    { new Guid("7B9E98FF-0015-4F9D-97D0-DFFF730C0B32"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Japanese", 0 },
                    { new Guid("42B884C6-5B8D-4CF5-B078-7F6C6EF6D2F5"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Jarai", 0 },
                    { new Guid("3DDF426D-5028-4094-8087-10286E80213A"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Javanese", 0 },
                    { new Guid("3E23EBD6-B781-4B0E-966A-3F4500B1D67C"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "K’iche’", 0 },
                    { new Guid("BA73E808-27DF-41E0-8F95-60DF93EB6B6D"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kabyle", 0 },
                    { new Guid("D71C39FC-4B51-4B9D-9554-1ADB84F79548"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kannada", 0 },
                    { new Guid("6D8C56A0-7007-4F79-BEFC-5076640634E2"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kashmiri", 0 },
                    { new Guid("05524B31-4DA5-49CA-A956-1E0439C73AAF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kazakh", 0 },
                    { new Guid("D200EB83-A75E-49C7-8A13-818EE1E73FDC"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Khmer", 0 },
                    { new Guid("E029EB84-1E66-4635-86AC-279EA5D5A957"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Khoekhoe", 0 },
                    { new Guid("AF198596-BC05-490F-AA33-0A26B94744EF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Korean", 0 },
                    { new Guid("4ED6C8D6-F639-4F94-8848-E36C72F7F950"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kurdish", 0 },
                    { new Guid("383D3719-B291-4C3E-B475-248E7AD77559"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Kyrgyz", 0 },
                    { new Guid("BD74718A-25DD-4377-B820-BE94F91846EE"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Lao", 0 },
                    { new Guid("93584A5C-C268-4A94-AC1C-C69887E9A7C2"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Latin", 0 },
                    { new Guid("5F4123A3-B699-426D-A8E7-6109721BD808"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Latvian", 0 },
                    { new Guid("BB6396DB-A4DA-4875-87D4-F26611DFADDB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Lingala", 0 },
                    { new Guid("0814EFC8-1B01-47EA-8CFD-7B413A7C47C2"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Lithuanian", 0 },
                    { new Guid("4EF0D520-C83B-408B-80DF-34CFAA966112"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Macedonian", 0 },
                    { new Guid("1864022C-A015-4344-9098-07D26050241B"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Maithili", 0 },
                    { new Guid("2B146AF7-5954-4E07-83D7-18A277344C9F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Malagasy", 0 },
                    { new Guid("AA833670-EB52-4448-B7E8-B280B51CBC35"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Malay (Bahasa Melayu)", 0 },
                    { new Guid("1F17D011-7C0C-4F22-AC12-51D4FE84960B"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Malayalam", 0 },
                    { new Guid("B269F6C1-91FE-4B40-AC91-57283E1455EA"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Mandarin (Chinese)", 0 },
                    { new Guid("0F5CA0BC-1599-4CCC-A6D3-2CE22425E338"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Marathi", 0 },
                    { new Guid("61392C16-3AC7-49F7-9BF4-A1F8052DAEEC"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Mende", 0 },
                    { new Guid("A937F3ED-600C-4584-9B25-EB7347289974"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Mongolian", 0 },
                    { new Guid("BB343768-0E27-42A3-AF64-4139E27DF7EB"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Nahuatl", 0 },
                    { new Guid("5F1C6B59-B3B0-41EB-915C-09D13EAE636C"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Navajo", 0 },
                    { new Guid("27D37E50-2D5F-4307-9E21-CAEB577C7890"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Nepali", 0 },
                    { new Guid("113D0406-4642-44D0-A6E2-F8E24F7C53E2"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Norwegian", 0 },
                    { new Guid("A18DB760-AA19-482D-B9BB-24328A16E57E"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Ojibwa", 0 },
                    { new Guid("B8657747-2A19-4F49-91D5-7C4675E9D383"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Oriya", 0 },
                    { new Guid("99BBED89-0DEA-4517-9D9B-175B5A86BF23"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Oromo", 0 },
                    { new Guid("330714C2-C6EF-40C1-8357-B093B77D8D39"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Pashto", 0 },
                    { new Guid("6B7C7759-823E-47F4-A840-0AE14BAF8BA9"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Persian", 0 },
                    { new Guid("5EF1CCE3-5101-424A-9A9A-BD4AA19CA72F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Polish", 0 },
                    { new Guid("01F8595D-F49F-43B2-A1DA-89E42C1BEF56"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Portuguese", 0 },
                    { new Guid("2597C468-1DDE-4FB0-893B-58FEC199AC7C"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Punjabi", 0 },
                    { new Guid("9E7D4503-C532-458E-91FF-426CF5EE8D09"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Quechua", 0 },
                    { new Guid("E9E37916-67C8-4553-BB3A-9AB7AA48E0A3"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Romani", 0 },
                    { new Guid("526CAE33-624F-442F-A7E3-574E617E2523"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Romanian", 0 },
                    { new Guid("148818A3-E896-4D5D-B36D-E08E154AD4C1"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Russian", 0 },
                    { new Guid("044A3E60-FB92-4A6E-A4E2-C6149615E30B"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Rwanda", 0 },
                    { new Guid("2E1248B0-5186-4100-A4B7-07845CC451DE"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Samoan", 0 },
                    { new Guid("F39B93E9-BCD1-40C5-A5AB-7C878AD2715F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Sanskrit", 0 },
                    { new Guid("DBBC11F0-ACB0-4789-9CE2-F862B5487079"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Serbian", 0 },
                    { new Guid("DD4F9F35-4D03-465E-9B26-26580DCB6A83"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Shona", 0 },
                    { new Guid("EB05A6B7-FF2B-4A07-B134-A3D36E2ECD24"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Sindhi", 0 },
                    { new Guid("E154ADE9-1ACE-461A-981A-CC2A13A66EAD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Sinhala", 0 },
                    { new Guid("B36CCA3B-A1A2-401B-A3CA-7BB4545DC0E4"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Slovak", 0 },
                    { new Guid("FF3A0EEE-4942-4732-BCF0-CEC6BF26E796"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Slovene", 0 },
                    { new Guid("AF25F349-BE3B-4066-925E-DC629F06E3B0"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Somali", 0 },
                    { new Guid("E92B6655-3083-4C6A-A978-E44D1B0FA945"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Spanish", 0 },
                    { new Guid("C892101D-D80B-4041-9E8C-C141ADD13C8B"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Swahili", 0 },
                    { new Guid("2678321C-B98A-430C-905C-5B8E58EFF583"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Swedish", 0 },
                    { new Guid("3B40A0AB-8F4D-4FA1-85A3-5D5C6D4BC8FF"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tachelhit", 0 },
                    { new Guid("CBB2416E-CBCB-4772-8BF8-98DAA1182E92"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tagalog", 0 },
                    { new Guid("166913D9-6AD0-462A-838C-014E2AB5CCAD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tajiki", 0 },
                    { new Guid("7B5919FD-39D1-4F05-BA96-CF14324CAF4D"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tamil", 0 },
                    { new Guid("9A80F2E3-7A27-41FD-B7E7-18DA6DBF206F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tatar", 0 },
                    { new Guid("A46188D6-3CBA-45A5-A827-9B69EC0CA135"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Telugu", 0 },
                    { new Guid("F03CFC46-9777-4953-8E2C-CC494BAB0163"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Thai", 0 },
                    { new Guid("0939A1A0-C376-41A5-BF59-E08A676F3B1F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tibetic Languages", 0 },
                    { new Guid("24329CDE-66AE-4D6F-AC88-94996E1506C5"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tigrigna", 0 },
                    { new Guid("D201D8F7-53D7-48B8-8BC6-A843278C971C"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Tok Pisin", 0 },
                    { new Guid("A34E4E0B-1B56-48C5-AE48-473C499A8115"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Turkish", 0 },
                    { new Guid("A45A7487-64BD-4B8D-AB6A-1B35C73947EA"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Turkmen", 0 },
                    { new Guid("819919AB-32C4-415F-BEDE-B4E32478A8C8"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Ukrainian", 0 },
                    { new Guid("5C4750D7-0F6D-446B-9401-1B062E85D68D"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Urdu", 0 },
                    { new Guid("A8661B13-A20E-4260-B1C2-E4725C008067"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Uyghur", 0 },
                    { new Guid("B557580C-1C60-4B2A-89BD-9762BFDC379C"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Uzbek", 0 },
                    { new Guid("FF61EC89-4E63-42A2-BA60-D9623AFEFF3D"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Vietnamese", 0 },
                    { new Guid("E33F34B4-0359-41A8-BB54-FE46518A7A4E"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Warlpiri", 0 },
                    { new Guid("19BC2B0B-CA63-4676-97A8-8A96650BFCCA"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Welsh", 0 },
                    { new Guid("7A896CEB-153A-4576-9731-6BA17368D49F"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Wolof", 0 },
                    { new Guid("CEFA28A4-27C2-4CCF-8FBE-A80C467362AD"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Xhosa", 0 },
                    { new Guid("F50D2340-D3E0-4667-B931-CD830F3E8688"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Yakut", 0 },
                    { new Guid("DAE7F833-3411-43C3-994D-BFFDFCBC7917"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Yiddish", 0 },
                    { new Guid("7D541562-8C7A-48BA-BC2E-ED8A3207BE5E"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Yoruba", 0 },
                    { new Guid("11DB3030-44A1-4521-8697-8FEAE0388544"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Yucatec", 0 },
                    { new Guid("FF7764A3-AF53-4532-A910-940C9FD379F0"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Zapotec", 0 },
                    { new Guid("7D75DFEC-D180-4821-90DD-A474B0B88461"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), "Zulu", 0 }
                },
                schema: "dbo");

            migrationBuilder.InsertData(
                table: "ProficiencyLevels",
                columns: new[] { "ProficiencyLevelGuid", "CreateDate", "CreateGuid", "IsDeleted", "Sequence", "ProficiencyLevelName" },
                values: new object[,]
                {
                    { new Guid("3BAD7FF3-B450-4837-A8A7-3E48FB08DD54"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), 0, 1, "Limited Working Proficiency" },
                    { new Guid("AD93BA96-186C-48B8-9159-59E9FDDA1CD1"), new DateTime(2020, 7, 2), new Guid("00000000-0000-0000-0000-000000000000"), 0, 2, "Bilingual Proficiency" }
                },
                schema: "dbo");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberLanguageProficiencies_LanguageId",
                table: "SubscriberLanguageProficiencies",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberLanguageProficiencies_ProficiencyLevelId",
                table: "SubscriberLanguageProficiencies",
                column: "ProficiencyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberLanguageProficiencies_SubscriberId",
                table: "SubscriberLanguageProficiencies",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberLanguageProficiencies");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "ProficiencyLevels");
        }
    }
}
