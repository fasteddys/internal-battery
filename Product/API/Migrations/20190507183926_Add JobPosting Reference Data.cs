using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddJobPostingReferenceData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[EducationLevel] ON");
          
            
            migrationBuilder.Sql("INSERT [dbo].[EducationLevel] ([IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EducationLevelId], [Level], [EducationLevelGuid]) VALUES (0, CAST(N'2019-05-07T13:10:51.2866667' AS DateTime2), CAST(N'2019-05-07T13:10:51.2866667' AS DateTime2), N'bfd144c7-2ee6-4e7a-94e9-cb601c3874cc', N'22fdafff-bc52-468c-bad2-71863ebe6aa9', 18, N'High School Diploma or Equivalent', N'f478a863-3c2f-4284-a66a-eba394306e0d')");

            migrationBuilder.Sql("INSERT [dbo].[EducationLevel] ([IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EducationLevelId], [Level], [EducationLevelGuid]) VALUES (0, CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), N'cdd580d4-f258-475a-99b1-3535b6583ad6', N'e160615d-bb4e-45f2-80ed-e0e31d7b9fa8', 19, N'Associate’s Degree', N'851ab1c8-e7c4-4f36-aedc-38a6e239f653')");

            migrationBuilder.Sql("INSERT [dbo].[EducationLevel] ([IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EducationLevelId], [Level], [EducationLevelGuid]) VALUES (0, CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), N'73638f09-9b21-4a09-bab0-76fc3bd501bd', N'46bc09aa-f603-4043-aa95-132379f947a7', 20, N'Bachelor’s Degree', N'07a97d32-6e17-42e7-bda2-8cdc07b6cff1')");

            migrationBuilder.Sql("INSERT [dbo].[EducationLevel] ([IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EducationLevelId], [Level], [EducationLevelGuid]) VALUES (0, CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), N'fed1c9de-c7fa-41a8-b30a-feeeb7783ea1', N'19db510a-430a-4df9-a77c-69456b6f411b', 21, N'Master’s Degree', N'ee4dd2e3-d1f6-4165-bd6b-26d1db58eb6e')");

            migrationBuilder.Sql("INSERT [dbo].[EducationLevel] ([IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EducationLevelId], [Level], [EducationLevelGuid]) VALUES (0, CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), CAST(N'2019-05-07T13:10:51.2900000' AS DateTime2), N'27860b49-dbf3-42fa-9710-48b49371dc0a', N'ae9dd47c-338e-4727-abf8-2bf51ef06198', 22, N'Ph.D. degree', N'c9386212-1d41-4e18-be80-3e0bc58e8349')");
            
            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[EducationLevel] OFF");
            

           
            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[EmploymentTypeId] ON");

            migrationBuilder.Sql("INSERT [dbo].[EmploymentType] ([EmploymentTypeId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EmploymentTypeGuid], [Name]) VALUES (19, 0, CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), N'bfa61813-f27d-40b6-925e-f31a0b8c16fe', N'e535e1b0-d3be-47fe-99e5-3afb3e6e9bb5', N'eb3f2db6-aaa9-4660-a92d-a11e4d83f23a', N'Full-Time')");

            migrationBuilder.Sql("INSERT [dbo].[EmploymentType] ([EmploymentTypeId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EmploymentTypeGuid], [Name]) VALUES (20, 0, CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), N'1492e7f3-1e28-4200-b357-c00afa62183a', N'c8b10272-c96a-4d15-b5b7-e80e868ba786', N'fa5744cd-d1c7-4f42-9a58-cf1b0fb28997', N'Part-Time')");

            migrationBuilder.Sql("INSERT [dbo].[EmploymentType] ([EmploymentTypeId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EmploymentTypeGuid], [Name]) VALUES (21, 0, CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), CAST(N'2019-05-07T13:49:59.5233333' AS DateTime2), N'660df752-042c-48c1-834b-fed538bafb99', N'b3c6b9cf-b074-4801-b5a0-5a5062643739', N'a3607e54-f5f7-4873-b4e8-6e7aec9198cd', N'Contractor')");

            migrationBuilder.Sql("INSERT [dbo].[EmploymentType] ([EmploymentTypeId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EmploymentTypeGuid], [Name]) VALUES (22, 0, CAST(N'2019-05-07T13:49:59.5266667' AS DateTime2), CAST(N'2019-05-07T13:49:59.5266667' AS DateTime2), N'6b2a3edc-4c31-4560-adb1-e42a47d5b076', N'4632faae-9d65-426d-841b-215e94b38a57', N'fb5f293c-24f3-4473-bf47-ea1f1455272c', N'Temporary')");

            migrationBuilder.Sql("INSERT [dbo].[EmploymentType] ([EmploymentTypeId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [EmploymentTypeGuid], [Name]) VALUES (23, 0, CAST(N'2019-05-07T13:49:59.5266667' AS DateTime2), CAST(N'2019-05-07T13:49:59.5266667' AS DateTime2), N'de0aecdf-986b-4081-9e37-e6c651bb2baa', N'45834c0a-9b46-4de3-b2d8-050a1a44ecd8', N'c912f22a-1376-482e-8a33-9c2868a4366d', N'Other')");

            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[EmploymentTypeId] OFF");
            
            
 
            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[ExperienceLevelId] ON");

            migrationBuilder.Sql("INSERT [dbo].[ExperienceLevel] ([ExperienceLevelId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [ExperienceLevelGuid], [DisplayName], [Code]) VALUES (23, 0, CAST(N'2019-05-07T13:17:12.3733333' AS DateTime2), CAST(N'2019-05-07T13:17:12.3733333' AS DateTime2), N'5564d546-26fb-499e-9da9-b87e985246a2', N'1606b74c-f2b8-470b-9049-0cb48d5cdcbf', N'c25235d2-a796-4da7-b66a-8d3df37279af', N'Intern', N'IN')");

            migrationBuilder.Sql("INSERT [dbo].[ExperienceLevel] ([ExperienceLevelId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [ExperienceLevelGuid], [DisplayName], [Code]) VALUES (24, 0, CAST(N'2019-05-07T13:17:12.3733333' AS DateTime2), CAST(N'2019-05-07T13:17:12.3733333' AS DateTime2), N'd00c4a5b-4672-4ac7-a06e-f419dcff8cef', N'8ca78ef5-a555-49cc-b1bb-daeeed53e985', N'7e621a60-d21a-4d2d-aa3f-55a476b1829b', N'Entry Level', N'EL')");

            migrationBuilder.Sql("INSERT [dbo].[ExperienceLevel] ([ExperienceLevelId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [ExperienceLevelGuid], [DisplayName], [Code]) VALUES (25, 0, CAST(N'2019-05-07T13:17:12.3766667' AS DateTime2), CAST(N'2019-05-07T13:17:12.3766667' AS DateTime2), N'47ec437c-b6e7-42cf-a48c-2af02e9c2fd7', N'782bae63-c0a4-4c0d-bec4-74fa48a08450', N'f105b4e4-643b-4beb-bdf3-9595508c5620', N'Mid-level', N'ML')");

            migrationBuilder.Sql("INSERT [dbo].[ExperienceLevel] ([ExperienceLevelId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [ExperienceLevelGuid], [DisplayName], [Code]) VALUES (26, 0, CAST(N'2019-05-07T13:17:12.3766667' AS DateTime2), CAST(N'2019-05-07T13:17:12.3766667' AS DateTime2), N'b2c254d2-4a7b-44ed-9086-f09cf2761db6', N'2110f77c-4b99-43b6-b090-720ff2df4a46', N'f3e95e74-1cc4-4547-8f29-11de077514c1', N'Senior Level', N'SL')");

            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[ExperienceLevelId] OFF");


           
            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[Industry] ON");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (18, N'db92b457-c16b-4588-94d5-69356a896d99', N'Architecture and Engineering', CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'e18a6390-9ab5-4aa6-992a-a4f784ccab35', 0, CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'c9cc9409-b8a1-44ef-b865-141fe5bdb4d6')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (19, N'fb8a1003-b7d2-48dc-9c36-99deee395088', N'Arts, Design, and Media', CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'157160fd-e04b-4f6c-8a40-874a28c27036', 0, CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'539a747a-430c-4e3a-8c1b-b8077ddd50a1')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (20, N'7ba33fa2-c2df-4310-884c-f7edaeeaa42f', N'Building Maintenance', CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'747ab238-bd67-40b8-ba86-a944d255d1c5', 0, CAST(N'2019-05-07T12:41:25.5500000' AS DateTime2), N'5e966c33-8be4-4d0b-99a2-d63f650e6666')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (21, N'0d8cd2c6-9b14-4b47-8fa4-36e3f102fbde', N'Business and Financial Operations', CAST(N'2019-05-07T12:41:25.5533333' AS DateTime2), N'796a9e38-2b67-4db2-ac0a-90224bd63aed', 0, CAST(N'2019-05-07T12:41:25.5533333' AS DateTime2), N'f49d62ff-27b4-431e-9b0e-bb8aa4a7130a')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (22, N'eb6fc5ce-3ee7-413f-8c02-3e6eb18b6fc7', N'Community and Social Service', CAST(N'2019-05-07T12:41:25.5533333' AS DateTime2), N'29da20d7-8bd3-4bad-9db2-b5709faabf62', 0, CAST(N'2019-05-07T12:41:25.5533333' AS DateTime2), N'cae083cb-8d69-4981-b26a-18c8cb9a9878')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (23, N'd912c2b3-5f48-45a3-932b-74392ffbe3db', N'Construction', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'6dcafda8-b0a6-4c63-ba27-5a967b04c291', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'34c78884-4470-4563-a7e7-b31b5deac12b')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (24, N'7d6e33ec-d657-4d69-a50a-7891c7ad49c0', N'Education', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'82361257-ee2a-4836-a807-ee28efcc2eeb', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'91e582db-6f9c-4f47-944e-d220b08147ea')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (25, N'b7a56aa3-1729-4c2e-b7b0-02689219c707', N'Farming, Fishing, and Forestry', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'3321cb1a-b0f9-4443-9a77-d2dee9dc4c99', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'32f94674-5c8e-45f6-bdab-af0c9e290ad2')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (26, N'7387f948-d89c-4468-8b4e-05bec72304d8', N'Food Services', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'bbb101e8-b493-48fb-ae74-a62f5692db60', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'fed5b96c-f2fc-4a14-a6b0-b2bc8048f39d')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (27, N'2791633a-6d34-4f41-849b-380fdbd60667', N'Healthcare Practitioners and Technical', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'1dd6d8ec-c1d8-475d-8c15-27e8bb1b4990', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'fc7fab32-453d-4a37-81d1-71ef407868f3')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (28, N'f34b8a53-363c-4140-b0dd-464bbbe16e95', N'Healthcare Support', CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'9dcb91de-a0ce-49b9-947d-b04a0d50611a', 0, CAST(N'2019-05-07T12:41:25.5566667' AS DateTime2), N'dd7f38ef-a2df-4039-aa26-7fc87d56e456')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (29, N'3545cd66-9ab4-4519-b86f-e5f1dc693a78', N'Installation, Maintenance, and Repair', CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'df5962bd-1abe-48af-8ab3-09c9ee9b2f62', 0, CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'f4348f43-6c18-44b0-880b-36a017f5a777')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (30, N'805b7834-2b82-44aa-bbde-0a3dd8171e37', N'Legal', CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'5e42d5e1-c9dd-4c13-9106-2db88f5555b3', 0, CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'f0d9b9f2-d143-419a-8c13-e0528134f718')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (31, N'23a9ca57-e4d1-45eb-bd00-6b9c20cf9332', N'Life, Physical, and Social Science', CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'5a2c04e8-f1b3-4261-9ed7-d59865049365', 0, CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'4a5cc16a-c80b-4c77-9ff8-fd525155a358')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (32, N'a2278e2b-c482-4c5d-8570-18c5bc3014ff', N'Management', CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'4b234f19-6197-4256-b8c2-39984ae8f4eb', 0, CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'afa66cde-f6aa-485d-933d-4f9d4d04d8ee')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (33, N'6aa5af6a-c057-40b6-8011-7410ffdc1825', N'Manufacturing and Production', CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'ce5c31c8-2318-467a-b80a-1bba8cc2ce14', 0, CAST(N'2019-05-07T12:41:25.5600000' AS DateTime2), N'3e57e859-4235-4791-8876-abe81e3d686b')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (34, N'06c87dcb-bba5-4408-b341-9650eadfbdfd', N'Office and Administrative Support', CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'a8b9d8f3-7678-4d94-9a3f-4df2f2badadf', 0, CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'3d1d5a83-3555-440b-b126-0afed1e53769')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (35, N'1f9e80d6-9c3b-4cab-b235-5288f0fa9a1d', N'Personal Care and Service', CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'f15500c5-1704-4925-9bb7-2bc771a1fc19', 0, CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'71089b60-1503-46ba-8237-a0597ee97f51')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (36, N'dae708cc-956d-4b0d-a698-8ef8fde1ab28', N'Protective Services', CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'bef874ab-47a4-413a-83dc-efb38870d6b2', 0, CAST(N'2019-05-07T12:41:25.5633333' AS DateTime2), N'01488f4b-9a48-4fd7-9146-41fb179e091a')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (37, N'33a801aa-2bc6-410a-9090-cdaddb75ff1d', N'Sales', CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'22529805-ea8e-4bba-bb1c-79af96cb90ca', 0, CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'47ab4467-5593-4966-ab16-9fcc4cc7c50e')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (38, N'6cad5ee1-d82b-48ab-a7a3-cbdabfc9b7f0', N'Technology', CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'235deb0b-aafc-45be-9146-079871d6e525', 0, CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'e737d9c4-b358-474f-832c-96c9b710765a')");

            migrationBuilder.Sql("INSERT [dbo].[Industry] ([IndustryId], [IndustryGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (39, N'c2d623d9-a814-4ddb-998f-4e91dec598c5', N'Transportation and Moving', CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'c3b61822-6f65-4f10-aa02-d1c27fa5d796', 0, CAST(N'2019-05-07T12:41:25.5666667' AS DateTime2), N'7ff99de9-d142-4375-9b24-012cf1b0bd2a')");

            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[Industry] OFF");


 
            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[JobCategory] ON");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (18, 0, CAST(N'2019-05-07T12:51:19.4166667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4166667' AS DateTime2), N'6da2719b-2d13-4752-8e5d-013dfdbe7456', N'1e7f9cbb-0fc9-415d-b52b-7e046aa7ec26', N'7becf388-cede-47a2-8de7-53ccd9e14cd8', N'Insurance')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (19, 0, CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), N'53fb7236-8547-47d5-8bf9-f709b2074495', N'398f9a37-c709-4116-82d2-5339d97368c3', N'043d994f-035a-4566-b970-dd50a5d5c13b', N'Inventory')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (20, 0, CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), N'b109e4bc-f10b-49fb-b3a8-3cb82e41c470', N'f950e207-eebe-4b6f-a6cd-1c6fd5c307f8', N'6da6829c-db83-4d35-98ae-c2cc1f1d17d7', N'Legal')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (21, 0, CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), N'b3895737-f251-429c-a243-57648aec0f5b', N'f1cfdd65-ea03-4c84-b57d-6432ca658983', N'283d1e51-ba9f-437b-a7cc-f294bcbedae1', N'Legal Admin')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (22, 0, CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), N'62a1c916-20e3-4780-a7e3-72bf11812c1b', N'e981378f-ce6a-4f56-aa77-da556c141142', N'f3490cee-a4b8-4eb7-a028-3b6ca4d32284', N'Management')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (23, 0, CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4200000' AS DateTime2), N'35eee674-79a0-497e-9dd3-bb8484497eb1', N'6220b106-c2a2-4705-9191-0e44ac8e037d', N'cee3de40-de13-409d-8b3c-cdab77d2abbc', N'Manufacturing')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (24, 0, CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), N'75492fc2-460c-4cb8-998f-1b69b3fc9699', N'bb31250a-780b-483e-a126-2ce5db5a8efc', N'4b9b41cd-8025-4a2b-858f-dd15a9cfe8e0', N'Marketing')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (25, 0, CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), N'1a9373d1-90b7-46f3-9b5b-dff478b09a49', N'0196ab49-9b22-4105-ac8c-fa8e2363f245', N'88101a3b-0023-4d65-8b52-92fb8b136f05', N'Media - Journalism - Newspaper')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (26, 0, CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), N'cafada5e-3b89-480f-88b2-2d9e2486948b', N'52fec105-db1d-423c-ade2-275c7827ac8a', N'05d1ec29-bf27-4d70-9e8c-2c8ececd4e54', N'Nonprofit - Social Services')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (27, 0, CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), N'e4a221ed-52f1-4929-b3e7-8939dc40c499', N'a23e82be-d936-4338-b198-ab1dab71d84f', N'030d06cf-ee99-45d6-9eb5-45d2db7ed2ca', N'Nurse')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (28, 0, CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), CAST(N'2019-05-07T12:51:19.4233333' AS DateTime2), N'b88dc072-b3ca-4fd7-9d56-53200e878c52', N'0ebfe4a2-a139-44df-9788-8bcabcbbac11', N'50fd9cea-59b4-4839-9e6b-a3cade23240e', N'Other')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (29, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'44745279-a4de-4681-9e4c-a15f78e419c5', N'8d33cd7b-0b13-4439-92bc-5721afd09c87', N'b0bbc3cb-8493-4b8a-a12f-6f818684acaa', N'Pharmaceutical')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (30, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'ac409792-99dd-429c-a3ec-cb66db7248be', N'ca35e980-a415-4edf-a0cf-bd04d62a89d2', N'e3b21e5c-4077-4a9d-baed-43ffb78eabda', N'Professional Services')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (31, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'2de1742f-3cb6-48e2-88e1-f995e2a389f5', N'39e0ea47-2812-43a4-aa43-78b71fe0dfaa', N'4ccc6c11-ac00-4367-9534-5a96e937ca08', N'Purchasing - Procurement')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (32, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'c421d67f-fe61-4cad-86c6-28ace4287195', N'34ac9043-fdc4-4120-9434-823a1036eafa', N'd5bf2463-74be-4bc9-a278-7c72f1e7b9cc', N'QA - Quality Control')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (33, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'0d05ec0c-cbb3-4153-b364-63138801d687', N'5e618670-69d1-4dc1-953f-6d3ab64501cd', N'3d16c063-6465-4f56-966f-5db466259fca', N'Real Estate')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (34, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'fd7bef34-0dc9-4d0a-afa3-59f947bc44b5', N'c7f5ba10-19be-4b45-acc8-935ee176d78f', N'38979b68-3e51-4a41-a983-dc66cf84e293', N'Research')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (35, 0, CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), CAST(N'2019-05-07T12:51:19.4266667' AS DateTime2), N'9a99add0-557e-4d78-b9cf-102acc16fb25', N'6d3524cf-62fb-4c17-9750-40e9b25912c6', N'e7cfbe17-9fa9-498a-825a-5545fb42000f', N'Restaurant - Food Service')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (36, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'eda20bbf-4323-473b-a61a-337c89a2dca0', N'c1939aa5-f118-458e-ad9e-aac292d289c1', N'e91f6a37-b204-499c-9141-4ec7c60e8c67', N'Retail')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (37, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'31d167ee-a782-4b82-9b20-74fe15b533f6', N'fe8e31fe-b2c1-4437-8496-50a4e9f7bd2a', N'2e106e92-a4e0-49fa-899d-9593dbc1fe09', N'Sales')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (38, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'3ab87353-2133-48a8-a92f-a9d6a2e7fbc7', N'33d9a23e-09e3-4f27-a3f5-4133588df910', N'602c8477-8091-46b3-b340-532ec8bcc085', N'Science')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (39, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'0592c109-be32-40e5-9ec4-5deacdd755b8', N'cba916d5-7d97-43fb-a9fc-4b341c2277ba', N'c5920289-e48e-4794-884b-4066d17ac238', N'Skilled Labor - Trades')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (40, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'42c0ce5f-2d0a-4016-aed6-22a3c9fed5b7', N'11219136-75e3-4115-96d1-fe668c5806fc', N'a4473733-44e7-4e1e-8c5b-39fd1e3a2659', N'Strategy - Planning')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (41, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'5393714e-c4ea-4bbf-aef5-03507548f18a', N'1f4f17fc-fb82-4938-8426-cedfc3a054df', N'6ccca0d2-bf4e-44fb-9ea7-fddbff9d6b7c', N'Supply Chain')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (42, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'f33416cf-9ec6-4066-bd37-15f215d0be3c', N'8ae7e22c-1b15-4f02-b8df-a62160217aef', N'fb27b353-841f-4db7-b25c-83e9d5d1e482', N'Telecommunications')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (43, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'c3076901-d04d-4f1b-a984-604e63331f5b', N'9fd77109-743d-47dc-b14e-c38235105db8', N'a99a0c00-48f9-40be-8f57-8a477f9ea725', N'Training')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (44, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'0820de90-c529-4336-8bd9-385318a1771b', N'958812ce-e7bf-4c89-9a1e-628d9d58d859', N'b8a220dd-d941-49e5-9235-4f1a903799ed', N'Transportation')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (45, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'1c909a98-28a6-4a34-b1ea-6fd20bd051c7', N'388b7d95-06b3-4c65-b940-971a893cf7d7', N'70d0b204-379b-46c0-aae9-11d77f02ebfb', N'Veterinary Services')");

            migrationBuilder.Sql("INSERT [dbo].[JobCategory] ([JobCategoryId], [IsDeleted], [CreateDate], [ModifyDate], [CreateGuid], [ModifyGuid], [JobCategoryGuid], [Name]) VALUES (46, 0, CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), CAST(N'2019-05-07T12:51:19.4300000' AS DateTime2), N'454e5f91-cccb-4b62-90b6-367c1f883695', N'23a5559a-5438-44c5-a08c-ca9c16b1c95a', N'd6b7c3f0-b9f8-438f-9b02-f3fefc80bb77', N'Warehouse')");

            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[JobCategory] OFF");


            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[SecurityClearance] ON");
            
            migrationBuilder.Sql("INSERT [dbo].[SecurityClearance] ([SecurityClearanceId], [SecurityClearanceGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (18, N'5770e1dd-0e8d-4528-bb1e-60081185ef17', N'Secret', CAST(N'2019-05-07T14:14:28.3233333' AS DateTime2), N'6aab637a-e064-4c62-9d0c-fa57562f4626', 0, CAST(N'2019-05-07T14:14:28.3233333' AS DateTime2), N'5d48810a-0a31-4ed9-a0e7-fa0c3e20e6df')");

            migrationBuilder.Sql("INSERT [dbo].[SecurityClearance] ([SecurityClearanceId], [SecurityClearanceGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (19, N'bb408457-0c62-4c93-b2dc-2351f07c15fb', N'Top Secret', CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'9c907009-ca24-4715-a24c-ae11c3f3e081', 0, CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'74810bb3-b2b5-4c27-9c15-ed7d1c881da6')");

            migrationBuilder.Sql("INSERT [dbo].[SecurityClearance] ([SecurityClearanceId], [SecurityClearanceGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (20, N'5ed78407-8375-4ccc-9a56-f6ccd33b9684', N'Public Trust', CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'87929a67-7dba-4f2c-9205-e804ccbf3626', 0, CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'58c6e1a9-40f5-4806-b502-7484644a51a6')");

            migrationBuilder.Sql("INSERT [dbo].[SecurityClearance] ([SecurityClearanceId], [SecurityClearanceGuid], [Name], [CreateDate], [CreateGuid], [IsDeleted], [ModifyDate], [ModifyGuid]) VALUES (21, N'5f57cfa2-f4aa-4e50-aa4f-8060b5b4161f', N'None', CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'6dc30508-9594-435c-976e-e9d39504b91a', 0, CAST(N'2019-05-07T14:14:28.3266667' AS DateTime2), N'61aef519-718e-4b2b-a905-c252bfb7fc94')");

            migrationBuilder.Sql("SET IDENTITY_INSERT[dbo].[SecurityClearance] OFF");
    
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from industry where industryid = 18");
            migrationBuilder.Sql("delete from industry where industryid = 19");
            migrationBuilder.Sql("delete from industry where industryid = 20");
            migrationBuilder.Sql("delete from industry where industryid = 21");
            migrationBuilder.Sql("delete from industry where industryid = 22");
            migrationBuilder.Sql("delete from industry where industryid = 23");
            migrationBuilder.Sql("delete from industry where industryid = 24");
            migrationBuilder.Sql("delete from industry where industryid = 25");

            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 18");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 19");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 20");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 21");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 22");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 23");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 24");
            migrationBuilder.Sql("delete from jobcategory where jobcategoryid = 25");

            migrationBuilder.Sql("delete from EducationLevel where EducationLevelID = 18");
            migrationBuilder.Sql("delete from EducationLevel where EducationLevelID = 19");
            migrationBuilder.Sql("delete from EducationLevel where EducationLevelID = 20");
            migrationBuilder.Sql("delete from EducationLevel where EducationLevelID = 21");
            migrationBuilder.Sql("delete from EducationLevel where EducationLevelID = 22");

            migrationBuilder.Sql("delete from ExperienceLevel where ExperienceLevelID = 23");
            migrationBuilder.Sql("delete from ExperienceLevel where ExperienceLevelID = 24");
            migrationBuilder.Sql("delete from ExperienceLevel where ExperienceLevelID = 25");
            migrationBuilder.Sql("delete from ExperienceLevel where ExperienceLevelID = 26");

            migrationBuilder.Sql("delete from EmploymentType where EmploymentTypeID = 19");
            migrationBuilder.Sql("delete from EmploymentType where EmploymentTypeID = 20");
            migrationBuilder.Sql("delete from EmploymentType where EmploymentTypeID = 21");
            migrationBuilder.Sql("delete from EmploymentType where EmploymentTypeID = 22");
            migrationBuilder.Sql("delete from EmploymentType where EmploymentTypeID = 23");

            migrationBuilder.Sql("delete from SecurityClearance where SecurityClearanceId = 18");
            migrationBuilder.Sql("delete from SecurityClearance where SecurityClearanceId = 19");
            migrationBuilder.Sql("delete from SecurityClearance where SecurityClearanceId = 20");
            migrationBuilder.Sql("delete from SecurityClearance where SecurityClearanceId = 21");
        }
    }
}
