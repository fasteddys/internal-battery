using System;
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
                    ProficiencyLevelName = table.Column<string>(maxLength: 500, nullable: false)
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
                    SubscriberLanguageProficienciesGuid = table.Column<Guid>(nullable: false),
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
                columns: new[] { "LanguageId", "CreateDate", "CreateGuid", "IsDeleted", "LanguageGuid", "LanguageName", "ModifyDate", "ModifyGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(714), new Guid("0cc58228-704c-4dd2-8002-77918633cf97"), 0, new Guid("21d11543-2251-47d1-8d88-5561ad6da116"), "Afrikaans", null, null },
                    { 97, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5586), new Guid("963f2896-0a32-4fec-ba7d-b5dab677e45f"), 0, new Guid("b4fab608-943e-41c8-b2fc-eacdde20eec6"), "Oriya", null, null },
                    { 98, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5588), new Guid("b4fdd97e-e506-4f9f-91f6-f65b188fbb70"), 0, new Guid("ddcfa4d6-7a6a-4a1c-861a-2b40bd694ab2"), "Oromo", null, null },
                    { 99, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5591), new Guid("f4d82db1-e284-4a2b-a1df-c1d74c934e59"), 0, new Guid("37266f80-3ce6-4cfe-ac98-e3b699ddadbf"), "Pashto", null, null },
                    { 100, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5593), new Guid("e3f64393-df19-4348-9e7a-585d73ac96dc"), 0, new Guid("a00028df-ca53-4ec4-8b55-07568f3cbe76"), "Persian", null, null },
                    { 101, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5595), new Guid("765f5220-1d31-4cd6-838e-c4e6c900e454"), 0, new Guid("191b3ec9-08d2-4f52-92d0-e3d9b8503ad3"), "Polish", null, null },
                    { 102, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5596), new Guid("553cfbda-ff5a-4a7c-b0ff-e77d701e4630"), 0, new Guid("25341b7e-6138-4fb8-aa0b-3e464a3a1a3d"), "Portuguese", null, null },
                    { 96, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5584), new Guid("6faead20-2ccd-4913-96c7-4b08be64d87f"), 0, new Guid("4f6b0b4d-bac5-4822-aa11-755dd9b7a3a6"), "Ojibwa", null, null },
                    { 103, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5600), new Guid("dee4bbfa-016c-4967-9405-a04118e4ec3f"), 0, new Guid("588fe0cf-1ac9-4731-9d7e-aa262eab7b67"), "Punjabi", null, null },
                    { 105, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5603), new Guid("d100ab55-34a4-4e08-9475-a421b82bb8f8"), 0, new Guid("e24798fb-36a1-47c5-8ea5-5b011fe9770c"), "Romani", null, null },
                    { 106, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5605), new Guid("aa5f5d12-13f7-4956-a169-88d1f2582dac"), 0, new Guid("e8b419ba-8f9f-4dcd-a19c-3b9d67af1650"), "Romanian", null, null },
                    { 107, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5609), new Guid("0a2b8921-89f1-4722-b2b4-bac447c4062a"), 0, new Guid("42f0b849-0d86-4149-877c-934cfcbf9db2"), "Russian", null, null },
                    { 108, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5610), new Guid("ec4e1ded-e240-4011-bdfa-a3ada0202736"), 0, new Guid("778f3b4b-c084-4963-ba2a-a06d5ca06743"), "Rwanda", null, null },
                    { 109, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5612), new Guid("7aac94f4-f4a6-42b5-a756-4ec5273c1945"), 0, new Guid("c29b76b7-afd8-4bf0-8f4e-b999fa32dfb6"), "Samoan", null, null },
                    { 110, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5614), new Guid("39c6ac0b-d20e-4190-8815-eae5452d808c"), 0, new Guid("e3899f67-0b31-4fd8-af40-810c7665d9ef"), "Sanskrit", null, null },
                    { 104, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5602), new Guid("d613ee5c-e174-49bc-a41c-dd370b8dc580"), 0, new Guid("f2309129-2d4e-4409-86aa-f11e0d083362"), "Quechua", null, null },
                    { 95, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5583), new Guid("9fc39a64-ae47-47c6-8ff9-10274a0abb16"), 0, new Guid("11c1bc5f-3a7a-49f2-80ae-65368c35733d"), "Norwegian", null, null },
                    { 94, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5579), new Guid("a8269a51-1f9d-4902-943f-b83c4a7ce28c"), 0, new Guid("3f89793a-96b1-4af5-b730-9b1329f7366e"), "Nepali", null, null },
                    { 93, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5578), new Guid("560390f3-68c6-4eaf-9f9d-e4853fc51b90"), 0, new Guid("0c17a7fc-97c2-480f-9458-07530e9a46e7"), "Navajo", null, null },
                    { 78, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5545), new Guid("d983292d-ce88-4560-bf3f-32f460d2ee2f"), 0, new Guid("ea6501ad-77af-4b77-aeff-2ff9f9f982bf"), "Lao", null, null },
                    { 79, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5548), new Guid("e1fd3f63-22bb-48cd-afac-212b4f749749"), 0, new Guid("4dcd610f-6c36-4b68-a523-23ed1f7cbf7f"), "Latin", null, null },
                    { 80, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5550), new Guid("8898e6c3-e4a0-4e5c-9318-f42cd50dc5a1"), 0, new Guid("6ca63106-3c77-4dc4-a42c-9810c9f9f92d"), "Latvian", null, null },
                    { 81, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5552), new Guid("14e39028-d815-4e0a-adf3-fbf31d492a6c"), 0, new Guid("99f64c73-6f33-4a32-859e-78d742d6ce1a"), "Lingala", null, null },
                    { 82, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5553), new Guid("04d98f2a-f544-42f8-9668-1d8a39a45162"), 0, new Guid("0f57d846-de0a-49cc-a724-319db6889b33"), "Lithuanian", null, null },
                    { 83, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5557), new Guid("48232903-c0b4-4210-a689-9c57076f79ed"), 0, new Guid("475c0325-df08-4949-b335-3499fd667e2f"), "Macedonian", null, null },
                    { 84, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5559), new Guid("bca1b399-8e49-4aea-857e-cdac09c83629"), 0, new Guid("cc83e30a-3ef1-438b-bf7d-6430c8cbc6cd"), "Maithili", null, null },
                    { 85, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5560), new Guid("098df1fe-0336-4cbd-9190-555f23b6b4fd"), 0, new Guid("21c48b97-8394-4289-a6a3-a0c661231285"), "Malagasy", null, null },
                    { 86, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5562), new Guid("ce00c63f-0ef5-4b6b-930f-1183fff7e7e1"), 0, new Guid("9f5ba765-d3a2-49e1-85a4-e2ed8c1f8ea6"), "Malay (Bahasa Melayu)", null, null },
                    { 87, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5566), new Guid("b8132258-9e5a-4b17-a529-b1d624177ed2"), 0, new Guid("e3fca79b-384a-4378-a2a0-6488f2602490"), "Malayalam", null, null },
                    { 88, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5567), new Guid("d2d920ab-9418-4509-9190-7dd5726453aa"), 0, new Guid("860c956a-ec11-419a-89ee-2f779eb702a9"), "Mandarin (Chinese)", null, null },
                    { 89, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5569), new Guid("592e8b9c-e957-4d94-8a6b-a8155ce9e405"), 0, new Guid("ed709f84-51b2-49e6-866f-3406f2d597fd"), "Marathi", null, null },
                    { 90, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5571), new Guid("5203d637-1869-4b20-a916-a9375b238bc5"), 0, new Guid("e8b04fb3-eb1f-4b1a-8ea6-4af12117c5c0"), "Mende", null, null },
                    { 91, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5574), new Guid("1a31d255-5bb4-4b98-b9cc-a0a7dac3b845"), 0, new Guid("ea9acc9e-424b-4d13-9123-7d894a735c91"), "Mongolian", null, null },
                    { 92, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5576), new Guid("d98941de-dd6c-434e-befa-721b590f1a29"), 0, new Guid("d19de8c7-de65-407b-b9a6-af2fd1316b65"), "Nahuatl", null, null },
                    { 111, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5617), new Guid("d9377596-fcdd-432e-a667-dca2e0f3df01"), 0, new Guid("7bd62f47-2c4c-434c-80b6-bc7cc46d5170"), "Serbian", null, null },
                    { 77, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5543), new Guid("8377b842-820f-4225-98d7-4cf8f29f39c6"), 0, new Guid("4b7a3a6a-d5f1-4736-9466-e2e7176cabbd"), "Kyrgyz", null, null },
                    { 112, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5619), new Guid("74f3c63a-b63f-4560-97dc-1f82f548c48b"), 0, new Guid("543e91c8-40f6-4766-ac07-7d606107bafe"), "Shona", null, null },
                    { 114, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5622), new Guid("c8bb4e6f-841a-407a-8e8f-a2b16dcfe7f5"), 0, new Guid("931d30b3-c748-4f49-b659-51be01447a9e"), "Sinhala", null, null },
                    { 134, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5698), new Guid("a38d83b2-fba9-459c-9382-23e68f25b07f"), 0, new Guid("152c3f04-c359-41fa-bafa-33fbd484701e"), "Urdu", null, null },
                    { 135, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5701), new Guid("69de6ec9-f95a-4b93-994e-a506628f0b48"), 0, new Guid("59962ed6-5d0f-4b06-a6ec-13a94d866c8b"), "Uyghur", null, null },
                    { 136, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5703), new Guid("ec6d6b3f-0c55-4c5a-8867-fbe5358ce9f8"), 0, new Guid("aa2acd13-c0ad-4158-99fd-42f8dd17eb9d"), "Uzbek", null, null },
                    { 137, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5705), new Guid("8b834479-5a8f-4cd2-8ea4-80e1f1471ae6"), 0, new Guid("66caacc6-f8d1-48be-9b34-8e978fe9b022"), "Vietnamese", null, null },
                    { 138, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5707), new Guid("c76e0728-fd86-4f6e-8ba5-70ed4f30c605"), 0, new Guid("d9749448-efa5-479f-8649-894266f1ba54"), "Warlpiri", null, null },
                    { 139, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5710), new Guid("468428f5-1b7e-429d-bd3f-57f901cf2936"), 0, new Guid("3a9bfd5f-bc8d-4536-920b-a65186dec73d"), "Welsh", null, null },
                    { 133, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5696), new Guid("27bab9ef-0e24-4027-9fb6-b1dba1b175d5"), 0, new Guid("dabd2cb6-fc01-42ee-91f2-53ff9d33ee46"), "Ukrainian", null, null },
                    { 140, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5712), new Guid("995cef29-6f64-4968-af17-317f9457b231"), 0, new Guid("bad19710-51c8-40bf-bd3d-7fdf327f0585"), "Wolof", null, null },
                    { 142, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5715), new Guid("84c65ab4-0abe-41f0-b30f-cc68ae5a000e"), 0, new Guid("0f931be3-bd39-4ca0-84eb-3cc5c2b18ccd"), "Yakut", null, null },
                    { 143, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5719), new Guid("c66ef210-c98c-46f4-a292-40cb4d39b147"), 0, new Guid("74681376-e668-4fd7-b172-324393c3dc3c"), "Yiddish", null, null },
                    { 144, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5720), new Guid("96bbb7c9-02db-445e-b730-385e1ebd4737"), 0, new Guid("41805026-0bdf-433d-877b-4b6b16a45fe3"), "Yoruba", null, null },
                    { 145, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5722), new Guid("08fa67f5-1234-4f33-827d-4ab8ce61c4af"), 0, new Guid("725176c9-62a6-416f-9019-aef87e0d42fd"), "Yucatec", null, null },
                    { 146, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5724), new Guid("15a84a76-9ad4-49d3-bfc3-0aa1f452c155"), 0, new Guid("5432b788-21b2-4cf0-a07e-d10fb687f1b7"), "Zapotec", null, null },
                    { 147, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5727), new Guid("54c958e7-3f8f-4cc9-b9e7-5e66f92256da"), 0, new Guid("ee3a814e-0000-4ca2-9e9b-933866691b4c"), "Zulu", null, null },
                    { 141, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5713), new Guid("390fb150-ead4-4ba2-bcf0-810e0c10e2d5"), 0, new Guid("1ba88130-600d-4405-aac0-07f0eb3a12b7"), "Xhosa", null, null },
                    { 132, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5695), new Guid("ceac8273-06a8-4deb-8ac8-0369844e7e3b"), 0, new Guid("e0466c15-2933-4372-a0d9-502715e06bf5"), "Turkmen", null, null },
                    { 131, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5693), new Guid("d6239a2f-07cf-42ca-bcb7-e49794eb475a"), 0, new Guid("c7c425d3-a65b-4631-a14f-c0e22f48ede1"), "Turkish", null, null },
                    { 130, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5689), new Guid("ed471094-4a2c-4ce2-8dfd-929b88755a5d"), 0, new Guid("cf547108-cf29-4333-9336-e5de3cced73a"), "Tok Pisin", null, null },
                    { 115, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5626), new Guid("84ac7f0c-72a7-4ed5-b35e-1fd07258da5d"), 0, new Guid("857b625a-633b-4bf2-8e2f-08120479d938"), "Slovak", null, null },
                    { 116, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5628), new Guid("de8bfe4c-119a-41ab-8aad-a6b8d199b71a"), 0, new Guid("1dac80d6-4c55-4c2e-814f-27fae4ddcdb3"), "Slovene", null, null },
                    { 117, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5629), new Guid("7eab67e9-1cc3-4e84-a989-413377e28beb"), 0, new Guid("f2eee25b-f3aa-49c7-9dab-c2e294d70199"), "Somali", null, null },
                    { 118, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5631), new Guid("63499295-372b-4b94-b4e8-639b0f826cad"), 0, new Guid("e660423f-6759-49c2-8b4e-0737a4e2a1df"), "Spanish", null, null },
                    { 119, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5634), new Guid("a9fadc8e-6bf3-463b-ad70-99f8410e3615"), 0, new Guid("67ee251a-4441-4dee-b91a-51451cbae49e"), "Swahili", null, null },
                    { 120, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5636), new Guid("b7a5c5b8-de7f-4acd-8145-1396fbd8299f"), 0, new Guid("1792d38b-84f5-42fc-8477-af18990b8684"), "Swedish", null, null },
                    { 121, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5638), new Guid("729af7e7-9e7e-4d15-a594-f600b1b7bd19"), 0, new Guid("2a1de813-602c-4ea8-ad6e-04835aa8e47e"), "Tachelhit", null, null },
                    { 122, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5639), new Guid("3cf9426b-81a8-48cd-9723-a5129612f9c6"), 0, new Guid("aa98ec5c-2747-4b33-8257-cb16e6122541"), "Tagalog", null, null },
                    { 123, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5643), new Guid("afdb6b59-c4be-41d6-9a4b-6688732b86f3"), 0, new Guid("908f3895-c1cc-4910-901e-5e1bb1fe6642"), "Tajiki", null, null },
                    { 124, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5645), new Guid("8e2ec2db-a22a-4508-b5f7-c61817d01038"), 0, new Guid("f6cbf7a9-82dd-4088-8dca-543d1ee351cf"), "Tamil", null, null },
                    { 125, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5646), new Guid("ce04443a-5160-4560-a43a-df7d621b658b"), 0, new Guid("4ed7ef22-abd1-402f-b48a-76c2efcd4881"), "Tatar", null, null },
                    { 126, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5648), new Guid("efc4b94a-a89a-4a11-bbe5-00adaf3bcb87"), 0, new Guid("7941f9a2-6c5c-44b8-87a5-9ac595681519"), "Telugu", null, null },
                    { 127, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5652), new Guid("4d9c10ad-16b9-426f-940a-f6069ecb38fb"), 0, new Guid("2d2eea5f-a847-4822-98b6-a7aa047598cc"), "Thai", null, null },
                    { 128, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5653), new Guid("662bec73-c198-4c2d-a65e-0df156aa4935"), 0, new Guid("74ae9543-b1d5-4601-9265-8d531876160e"), "Tibetic Languages", null, null },
                    { 129, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5655), new Guid("1373c4bc-9823-459f-a0f6-3a117b459b92"), 0, new Guid("cb7ce33d-f737-49bb-b216-7e41edb3fb09"), "Tigrigna", null, null },
                    { 113, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5621), new Guid("d3d1101d-6a90-4835-a8a6-4ccea45d74f9"), 0, new Guid("29ef290f-8d86-4175-94be-1d062e177fe5"), "Sindhi", null, null },
                    { 75, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5539), new Guid("791dd725-ea51-426b-a712-5fb8636f81ff"), 0, new Guid("ff142cf6-544c-44b8-aaa0-936eb302afd8"), "Korean", null, null },
                    { 76, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5541), new Guid("98a155ee-e8c9-4058-afcc-129c20dde54c"), 0, new Guid("95268ed1-7dfc-46a9-8a80-87a487d20f99"), "Kurdish", null, null },
                    { 73, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5534), new Guid("d9ad7d45-a4b6-4740-a0b0-7843561160f4"), 0, new Guid("9c412217-c4d9-47e9-b26e-90c0b295b0a6"), "Khmer", null, null },
                    { 21, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5383), new Guid("fe7c585b-3395-4277-b685-632ee82985e3"), 0, new Guid("23227f7b-39f5-4fb0-92a5-a15de1134650"), "Bislama", null, null },
                    { 22, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5385), new Guid("330072ac-9e29-410c-97bf-04838294a7f5"), 0, new Guid("66ab8789-fb34-492e-9c1d-78b0a2f4ecf9"), "Bosnian", null, null },
                    { 23, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5388), new Guid("04fbb35f-3d16-4e75-b039-5549e4c49eb4"), 0, new Guid("d1179720-6490-4844-8f1d-93d9445edf5b"), "Brahui", null, null },
                    { 24, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5390), new Guid("929e71e2-f067-4da1-ba25-3260fbaf41f7"), 0, new Guid("4c129b7a-3132-4ece-861d-b99f6a258188"), "Bulgarian", null, null },
                    { 25, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5392), new Guid("8e3f0575-461a-4bc6-9495-e0f4ab0088be"), 0, new Guid("bb896b2f-014b-476d-a3c2-99d722b0973e"), "Burmese", null, null },
                    { 26, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5394), new Guid("5ca23afa-b41d-4826-afc1-34716e8864c8"), 0, new Guid("1f4a9da5-71f1-41da-8825-42bbd0eec45c"), "Cantonese", null, null },
                    { 27, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5397), new Guid("9899ca15-675a-47d9-9e29-977bad4d6646"), 0, new Guid("de49fb33-6dc5-4c83-ab46-526a140d1c1d"), "Catalan", null, null },
                    { 28, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5399), new Guid("9c083aa8-20b4-413c-99fa-90df092433e4"), 0, new Guid("35a59708-571a-4db4-872d-78a4be90070d"), "Cebuano", null, null },
                    { 29, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5401), new Guid("75ab980e-82a8-40cd-b7e8-f48015fedff4"), 0, new Guid("8e8aa0ff-af7f-4987-b434-7cf4e887992f"), "Chechen", null, null },
                    { 30, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5402), new Guid("fc5e5c93-bf6f-40b7-b511-4316e4c213f1"), 0, new Guid("cd3a852d-0ed5-42d8-9112-289f5460cc84"), "Cherokee", null, null },
                    { 31, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5406), new Guid("e6a656cd-a56e-4129-8038-88893e9ba51e"), 0, new Guid("5d49a6c7-d8b9-4264-ac81-73b29144ff79"), "Croatian", null, null },
                    { 32, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5408), new Guid("57bc0a4b-520c-4c94-ae13-c3846b58c3af"), 0, new Guid("56df3c0d-7ff7-42a8-ac88-739c4cc31471"), "Czech", null, null },
                    { 33, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5409), new Guid("ed8d591c-5b1b-44db-8a7d-0b9ec9ecdad6"), 0, new Guid("aeed59e0-e35b-490f-85bb-e9f51e273be9"), "Dakota", null, null },
                    { 34, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5412), new Guid("fe4b6ffe-ff22-4c7a-ac5e-e01590a2f6a2"), 0, new Guid("60243f94-0777-4465-95a8-8e79bf1cf826"), "Danish", null, null },
                    { 35, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5415), new Guid("48ee3964-5753-444a-988b-a06435ddc166"), 0, new Guid("921cd19b-2638-4bda-a450-3933d67467e7"), "Dari", null, null },
                    { 20, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5381), new Guid("12428e99-ab84-4745-be38-a6c8e57d0fbc"), 0, new Guid("be12697c-5366-4f75-a8cd-3296c1cff958"), "Bhojpuri", null, null },
                    { 74, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5536), new Guid("208b2572-5ba5-4225-8752-d51d9ca9a741"), 0, new Guid("69b94830-a2ee-478c-b47f-d70798c5f8b1"), "Khoekhoe", null, null },
                    { 19, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5380), new Guid("4e8d6b3f-d597-4471-a0d5-84d89242c4fa"), 0, new Guid("4f513716-a162-4eea-8583-8ca10c03d985"), "Bengali", null, null },
                    { 17, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5374), new Guid("ffd7b036-1b43-463a-80a9-04687208e81b"), 0, new Guid("1fbf7734-b0c0-4522-a5eb-d282454a9e0f"), "Basque", null, null },
                    { 2, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5068), new Guid("9d3cd97f-5fd9-4cc3-a17d-854875c0d369"), 0, new Guid("546e3ad3-4a8e-478c-b6a2-682e14fd3ebc"), "Albanian", null, null },
                    { 3, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5275), new Guid("e582b091-f17d-474e-9b0f-75ea7211b0e7"), 0, new Guid("267e4850-afd8-4c30-ab2f-d3127a904079"), "Amharic", null, null },
                    { 4, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5280), new Guid("af716414-2de2-467d-bf4c-1031cdfe3cac"), 0, new Guid("b9cf8453-871b-4fff-a0b7-faadc99cc909"), "Arabic (Egyptian Spoken)", null, null },
                    { 5, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5282), new Guid("b3f37546-f501-48c6-8e10-bbdc8769aa2f"), 0, new Guid("9385889d-f83a-46af-82b2-b3522b40a05c"), "Arabic (Levantine)", null, null },
                    { 6, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5288), new Guid("6219a7ec-365c-484b-96f8-87515d8cb37a"), 0, new Guid("d2c9c916-6b5e-48da-9cd8-5a1778dcb9d2"), "Arabic (Modern Standard)", null, null },
                    { 7, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5293), new Guid("b9e7253c-7de3-41a9-aabd-7821612494d6"), 0, new Guid("ac26b9f3-346f-48f0-bd09-bac26d7fa2c0"), "Arabic (Moroccan Spoken)", null, null },
                    { 8, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5295), new Guid("bf008a72-f86d-4cd6-b4a4-37480ac99217"), 0, new Guid("3d17bd0d-4411-4172-ae9a-5eac9742e2a9"), "Arabic (Overview)", null, null },
                    { 9, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5297), new Guid("fdf5131c-ca62-4947-b23f-6c73f6e8fc8f"), 0, new Guid("b81f7955-e18b-4d57-9585-5452ade6b216"), "Aramaic", null, null },
                    { 10, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5357), new Guid("67bfc02c-d39c-4342-ae01-dbd3e659f6eb"), 0, new Guid("a43a50f3-7bb3-4db5-8b9a-b26d0fb5573d"), "Armenian", null, null },
                    { 11, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5361), new Guid("ba5d6110-0d1d-4bea-9f99-e4913b92a1aa"), 0, new Guid("12936988-7f01-4584-a1c0-ae48cf102405"), "Assamese", null, null },
                    { 12, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5363), new Guid("0612d853-0586-4231-893f-63b7c1a2d569"), 0, new Guid("75c04225-8e89-4de8-a5f0-32de7e404c40"), "Aymara", null, null },
                    { 13, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5365), new Guid("81cbd5a5-2e99-4c87-b4bd-ae3b6f689501"), 0, new Guid("09353ef6-de90-4814-bcf6-b2607269622f"), "Azerbaijani", null, null },
                    { 14, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5367), new Guid("841c3fdb-4c10-4e4e-9f33-bd25f03d8263"), 0, new Guid("0ca174d7-0f9c-4b57-bab6-5b74478bc12b"), "Balochi", null, null },
                    { 15, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5370), new Guid("ef8e5b86-a6f3-40fb-a381-c3fe84beff01"), 0, new Guid("e39ea499-dd0f-485f-9185-19eb22323675"), "Bamanankan", null, null },
                    { 16, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5372), new Guid("8ac790ae-febb-4d68-a645-9dc56687e0d3"), 0, new Guid("a424cab6-fe7e-452e-94e4-08fd1ffb96ff"), "Bashkort (Bashkir)", null, null },
                    { 18, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5376), new Guid("41559152-ed6f-4133-b1ef-2f52ded27c87"), 0, new Guid("10b44a08-e91a-45ea-8f81-88f37383422d"), "Belarusan", null, null },
                    { 37, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5418), new Guid("8f43e569-8568-435b-ae8c-c23c4644d708"), 0, new Guid("8d3460a2-127c-4007-a93c-4a7f7dde890a"), "Dutch", null, null },
                    { 36, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5417), new Guid("28ddcc83-7e41-4c0a-a942-a177b00605dd"), 0, new Guid("22180047-dadb-47a1-a612-909ed99ed6d3"), "Dholuo", null, null },
                    { 39, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5424), new Guid("f6a13b2c-7a39-4b81-a1b4-24c82cd0ec1a"), 0, new Guid("6784b357-5095-4e9e-9892-4a1656acb00a"), "Esperanto", null, null },
                    { 59, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5467), new Guid("4e91dfaa-a049-454d-a4c7-255677ba5c72"), 0, new Guid("69c4ae12-623c-418f-9c7e-4a0190fd6a23"), "Igbo", null, null },
                    { 60, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5469), new Guid("d44c1730-2c2e-42af-92bf-23ffc5879399"), 0, new Guid("aba2ea27-9a57-4975-9211-b6c47ae17dc9"), "Ilocano", null, null },
                    { 61, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5470), new Guid("2970967b-ee95-4b68-917d-9b1d8aca542c"), 0, new Guid("e02d46b7-95df-4475-8181-27dbf70f4b54"), "Indonesian (Bahasa Indonesia)", null, null },
                    { 62, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5472), new Guid("8aff8afb-0b26-494d-ac4b-55cb5b1f1a91"), 0, new Guid("d5c90e8b-799f-499a-a115-a9e9aff42cc5"), "Inuit/Inupiaq", null, null },
                    { 63, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5475), new Guid("310fd944-d21b-4e15-8612-d66f045c8e2b"), 0, new Guid("9fbcfe62-796f-4b47-a785-450ae2300de1"), "Irish Gaelic", null, null },
                    { 64, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5477), new Guid("25afd066-65c5-4033-ab46-6ae31e83559d"), 0, new Guid("2c402e8d-5ca7-4fa4-9f17-11b4e1c316db"), "Italian", null, null },
                    { 38, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5420), new Guid("f6cbbfc7-190d-4e41-9867-ac4fe145be2c"), 0, new Guid("6916f5c6-da7e-466a-bb20-b5a8740516f5"), "English", null, null },
                    { 65, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5479), new Guid("e0dd2f42-a649-40b2-b720-04929373cab6"), 0, new Guid("a47e42b4-c767-45fd-a69d-1c5ba8045c9d"), "Japanese", null, null },
                    { 67, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5522), new Guid("5ad925bd-51a4-4e62-be25-37802fc79ca5"), 0, new Guid("38bab8e1-1faa-48f7-8d38-dd7f6392ac5c"), "Javanese", null, null },
                    { 68, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5524), new Guid("65ad684c-16da-4a8e-80a5-ed7856319c07"), 0, new Guid("9c9f7e3c-e62d-4205-8f30-6de20d2ffb3c"), "K’iche’", null, null },
                    { 69, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5526), new Guid("6afee934-40ec-4c2a-9283-72838ae0804c"), 0, new Guid("0f40fc17-394b-4bdf-a3ad-0dbd5fe88305"), "Kabyle", null, null },
                    { 70, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5527), new Guid("fc1db583-ccaa-480c-9910-ec483d511862"), 0, new Guid("547114ac-ed43-4f55-ae1c-a66b4cae5a73"), "Kannada", null, null },
                    { 71, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5531), new Guid("50ea6fd0-3628-4c2e-92a9-b8f5e72d82b7"), 0, new Guid("a3af4ef8-c903-4d4a-a0c2-6ca2252c2150"), "Kashmiri", null, null },
                    { 72, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5532), new Guid("6102903a-e3b9-4127-9396-095f5bfab7f1"), 0, new Guid("0d0c3013-6770-48d8-acd1-2e7033fc8fac"), "Kazakh", null, null },
                    { 66, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5518), new Guid("43b5dd6b-8da6-4df8-bd79-c47da6632549"), 0, new Guid("e5862113-802d-43fb-850b-8e5050fc785b"), "Jarai", null, null },
                    { 57, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5462), new Guid("51d7d092-f08a-451c-85c5-eaddef47dac9"), 0, new Guid("cbe554eb-b1af-443d-bda3-0bd87d64314f"), "Hungarian", null, null },
                    { 58, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5463), new Guid("649f73ca-159f-402e-aabd-c0390ab30c42"), 0, new Guid("a9e9e0bc-6640-4030-8cd6-5fee5623be66"), "Icelandic", null, null },
                    { 55, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5458), new Guid("62ffd98d-2e91-4ea2-9a3d-901f87dd96cd"), 0, new Guid("bc7dafb4-90da-435e-a0f6-1f1169ea76be"), "Hiligaynon", null, null },
                    { 40, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5425), new Guid("3ef90314-fd2d-41a6-9499-087d43ab8454"), 0, new Guid("6fe23e2d-05c5-41e9-96be-1a95c584184b"), "Estonian", null, null },
                    { 41, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5427), new Guid("f7c01d3c-222c-4188-a976-9582556d5c78"), 0, new Guid("f229403f-de30-4f7c-a57c-218c0a55036a"), "Éwé", null, null },
                    { 42, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5429), new Guid("ba61ce91-45ee-4f38-a149-a5163c226751"), 0, new Guid("90d25c21-4e9a-4f9c-bfb3-4c3c27d44dc2"), "Finnish", null, null },
                    { 43, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5432), new Guid("52dc6bb8-90fc-47f1-8e04-5994a0a00a29"), 0, new Guid("4097139b-9dda-4dfc-93f0-56a1672b9138"), "French", null, null },
                    { 44, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5434), new Guid("c304ae38-0c02-4dab-82d2-b38f16763812"), 0, new Guid("4c60a54f-ded4-4a4d-bb88-fa1f4166dc63"), "Georgian", null, null },
                    { 56, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5460), new Guid("fb7249e8-1300-4531-ab62-da27991940e1"), 0, new Guid("e3a1bfd4-e60b-4110-b430-6f2f55e4ef99"), "Hindi", null, null },
                    { 46, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5438), new Guid("ade543c2-2d05-4a80-8581-d5faa674ace4"), 0, new Guid("d872050a-215f-47fa-b371-4176dac13f6a"), "Gikuyu", null, null },
                    { 45, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5436), new Guid("756c9705-0f30-48f8-b4c5-5498801a81cd"), 0, new Guid("6cd7dbe0-6c42-42b1-b514-43380b689ad8"), "German", null, null },
                    { 48, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5443), new Guid("d02b6500-9bd8-4e24-a441-9c52a6ceb65c"), 0, new Guid("94c8aacd-99c7-423b-9f8a-a7510e21efb2"), "Guarani", null, null },
                    { 49, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5445), new Guid("139cccbc-85e1-46de-ae65-9c73e2966671"), 0, new Guid("8bbdd6fd-24a9-4488-a4cb-61e8a397133c"), "Gujarati", null, null },
                    { 50, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5446), new Guid("b0e13e83-42b6-4d11-a954-17b18c6db80c"), 0, new Guid("b3e08f9d-f66b-46b9-b3cf-0c713543a92d"), "Haitian Creole", null, null },
                    { 51, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5450), new Guid("44871cab-b33b-4954-94b9-070fe1f0f37b"), 0, new Guid("46fb848f-44e6-4fea-816c-cdfb2a316789"), "Hausa", null, null },
                    { 52, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5451), new Guid("ced9ec1b-393e-4d7d-8573-fbd963e6ae6f"), 0, new Guid("d54dea6c-f2c0-4857-9e12-d2b380f46ccf"), "Hawaiian", null, null },
                    { 53, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5453), new Guid("2097a851-e097-444c-a993-280a1342acd9"), 0, new Guid("de778801-39c6-4438-bbb1-5352185034ab"), "Hawaiian Creole", null, null },
                    { 54, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5455), new Guid("3cdff5b8-ef77-4793-a019-8f816c22e012"), 0, new Guid("04e9b57b-3ba0-4630-8dd4-ed92197354f6"), "Hebrew", null, null },
                    { 47, new DateTime(2020, 6, 29, 13, 56, 28, 201, DateTimeKind.Utc).AddTicks(5441), new Guid("20871b10-4fc4-47b3-a7c4-6a51165b7059"), 0, new Guid("4c3cfb51-bede-4d65-9c00-c62924327c78"), "Greek", null, null }
                });

            migrationBuilder.InsertData(
                table: "ProficiencyLevels",
                columns: new[] { "ProficiencyLevelId", "CreateDate", "CreateGuid", "IsDeleted", "ModifyDate", "ModifyGuid", "ProficiencyLevelGuid", "ProficiencyLevelName" },
                values: new object[,]
                {
                    { 4, new DateTime(2020, 6, 29, 13, 56, 28, 202, DateTimeKind.Utc).AddTicks(2618), new Guid("d1538c7e-9815-4e8a-a30d-bc783d69cfe8"), 0, null, null, new Guid("d1349f97-f98d-4748-aa0d-0e9a6bb5f563"), "Real Good" },
                    { 1, new DateTime(2020, 6, 29, 13, 56, 28, 202, DateTimeKind.Utc).AddTicks(1529), new Guid("1a41920a-35ad-4bd9-9a6b-4d4344690238"), 0, null, null, new Guid("144291dd-0a2a-4371-8702-3930fa15d35b"), "Nope" },
                    { 2, new DateTime(2020, 6, 29, 13, 56, 28, 202, DateTimeKind.Utc).AddTicks(2595), new Guid("4a6cf6f8-0437-4d91-b309-05c3a27da8be"), 0, null, null, new Guid("ca703e6c-2fb8-44a8-9744-118c85bdd9d5"), "Meh" },
                    { 3, new DateTime(2020, 6, 29, 13, 56, 28, 202, DateTimeKind.Utc).AddTicks(2607), new Guid("1f158ffa-ea43-47df-a150-08fa8231e491"), 0, null, null, new Guid("e9cf61ab-60b4-4cb3-a672-91e4826f9aa1"), "Adequate" },
                    { 5, new DateTime(2020, 6, 29, 13, 56, 28, 202, DateTimeKind.Utc).AddTicks(2620), new Guid("133ce652-50eb-4024-90ab-924f93c794c1"), 0, null, null, new Guid("9070145f-eb7c-4765-9ec5-3d18c2b0e3ab"), "Amazing" }
                });

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
