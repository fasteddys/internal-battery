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
                    DisplayOrder = table.Column<int>(nullable: false)
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

            var languageSeedData = new[]
            {
                "Afrikaans", "Albanian", "Amharic", "Arabic (Egyptian Spoken)",
                "Arabic (Levantine)", "Arabic (Modern Standard)",
                "Arabic (Moroccan Spoken)", "Arabic (Overview)", "Aramaic",
                "Armenian", "Assamese", "Aymara", "Azerbaijani", "Balochi",
                "Bamanankan", "Bashkort (Bashkir)", "Basque", "Belarusan",
                "Bengali", "Bhojpuri", "Bislama", "Bosnian", "Brahui",
                "Bulgarian", "Burmese", "Cantonese", "Catalan", "Cebuano",
                "Chechen", "Cherokee", "Croatian", "Czech", "Dakota", "Danish",
                "Dari", "Dholuo", "Dutch", "English", "Esperanto", "Estonian",
                "Éwé", "Finnish", "French", "Georgian", "German", "Gikuyu",
                "Greek", "Guarani", "Gujarati", "Haitian Creole", "Hausa",
                "Hawaiian", "Hawaiian Creole", "Hebrew", "Hiligaynon", "Hindi",
                "Hungarian", "Icelandic", "Igbo", "Ilocano",
                "Indonesian (Bahasa Indonesia)", "Inuit/Inupiaq",
                "Irish Gaelic", "Italian", "Japanese", "Jarai", "Javanese",
                "K’iche’", "Kabyle", "Kannada", "Kashmiri", "Kazakh", "Khmer",
                "Khoekhoe", "Korean", "Kurdish", "Kyrgyz", "Lao", "Latin",
                "Latvian", "Lingala", "Lithuanian", "Macedonian", "Maithili",
                "Malagasy", "Malay (Bahasa Melayu)", "Malayalam",
                "Mandarin (Chinese)", "Marathi", "Mende", "Mongolian",
                "Nahuatl", "Navajo", "Nepali", "Norwegian", "Ojibwa", "Oriya",
                "Oromo", "Pashto", "Persian", "Polish", "Portuguese",
                "Punjabi", "Quechua", "Romani", "Romanian", "Russian",
                "Rwanda", "Samoan", "Sanskrit", "Serbian", "Shona", "Sindhi",
                "Sinhala", "Slovak", "Slovene", "Somali", "Spanish", "Swahili",
                "Swedish", "Tachelhit", "Tagalog", "Tajiki", "Tamil", "Tatar",
                "Telugu", "Thai", "Tibetic Languages", "Tigrigna", "Tok Pisin",
                "Turkish", "Turkmen", "Ukrainian", "Urdu", "Uyghur", "Uzbek",
                "Vietnamese", "Warlpiri", "Welsh", "Wolof", "Xhosa", "Yakut",
                "Yiddish", "Yoruba", "Yucatec", "Zapotec", "Zulu"
            }
                .Select((language, index) => new object[]
                {
                                index + 1, // LanguageId
                                DateTime.UtcNow, // CreateDate
                                Guid.Empty, // CreateGuid
                                0, // IsDeleted
                                Guid.NewGuid(), // LanguageGuid
                                language, // LanguageName
                                null, // ModifyDate,
                                null // ModifyGuid
                })
                .ToArray();


            var proficiencyLevelSeedData = new[] { "Limited Working Proficiency", "Bilingual Proficiency" }
                .Select((proficiency, index) => new object[]
                {
                    index + 1, // ProficiencyLevelId
                    DateTime.UtcNow, // CreateDate
                    Guid.Empty, // CreateGuid
                    index + 1, // DisplayOrder
                    0, // IsDeleted
                    null, // ModifyDate,
                    null, // ModifyGuid
                    Guid.NewGuid(), // ProficiencyLevelGuid
                    proficiency // ProficiencyLevelName
                })
                .ToArray();


            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "LanguageId", "CreateDate", "CreateGuid", "IsDeleted", "LanguageGuid", "LanguageName", "ModifyDate", "ModifyGuid" },
                values: languageSeedData,
                schema: "dto");

            migrationBuilder.InsertData(
                table: "ProficiencyLevels",
                columns: new[] { "ProficiencyLevelId", "CreateDate", "CreateGuid", "DisplayOrder", "IsDeleted", "ModifyDate", "ModifyGuid", "ProficiencyLevelGuid", "ProficiencyLevelName" },
                values: proficiencyLevelSeedData,
                schema: "dto");

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
