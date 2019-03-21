using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class subscriber_search_fulltext_improvement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE FULLTEXT CATALOG CCSearchCat
                    WITH ACCENT_SENSITIVITY = ON
                GO

                CREATE FULLTEXT INDEX ON [dbo].[Company] (
                    CompanyName Language 1033 
                ) 
                KEY INDEX PK_Company
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[EducationalDegree] (
                    Degree Language 1033 
                ) 
                KEY INDEX PK_EducationalDegree
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[EducationalDegreeType] (
                    DegreeType Language 1033 
                ) 
                KEY INDEX PK_EducationalDegreeType
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[EducationalInstitution] (
                    Name Language 1033 
                ) 
                KEY INDEX PK_EducationalInstitution
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[Skill] (
                    SkillName Language 1033 
                ) 
                KEY INDEX PK_Skill
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[Subscriber] (
                    FirstName Language 1033 
                    ,LastName Language 1033 
                    ,Email Language 1033 
                    ,PhoneNumber Language 1033 
                    ,Address Language 1033 
                    ,ProfileImage Language 1033 
                    ,City Language 1033 
                    ,FacebookUrl Language 1033 
                    ,GithubUrl Language 1033 
                    ,LinkedInUrl Language 1033 
                    ,StackOverflowUrl Language 1033 
                    ,TwitterUrl Language 1033 
                    ,PostalCode Language 1033 
                ) 
                KEY INDEX PK_Subscriber
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO

                CREATE FULLTEXT INDEX ON [dbo].[SubscriberWorkHistory] (
                    Title Language 1033 
                    ,JobDecription Language 1033 
                ) 
                KEY INDEX PK_SubscriberWorkHistory
                ON CCSearchCat
                WITH CHANGE_TRACKING AUTO
                GO
            ", true);

            migrationBuilder.Sql(@"Exec('
                CREATE VIEW dbo.v_SubscriberSearch with schemabinding
                as 
                select  
                    [SubscriberId], 
                    isnull([FirstName],'''') +'' '' + isnull([LastName],'''')  +'' '' + isnull([Email],'''')  +'' '' +REPLACE(isnull([Email],''''),''@'','' '')  +'' '' + isnull([PhoneNumber],'''') +'' '' +
                    replace(isnull([PhoneNumber],''''),'')'','''') +'' '' + replace(isnull([PhoneNumber],''''),''('','''') +'' '' + replace(isnull([PhoneNumber],''''),''-'','''') +'' '' + 
                    isnull([Address],'''')  +'' '' +  isnull([City],'''')  +'' '' +  isnull([FacebookUrl],'''')+'' ''+ isnull([GithubUrl],'''')  +'' '' + isnull([LinkedInUrl],'''')  +'' '' +
                    isnull([StackOverflowUrl],'''')  +'' '' + isnull([TwitterUrl],'''')  +'' '' + isnull([PostalCode],'''') AS SearchString from dbo.Subscriber
                where isdeleted = 0
            ')");

            migrationBuilder.Sql(@"
                CREATE UNIQUE CLUSTERED INDEX UCI_SearchView ON dbo.v_SubscriberSearch (SubscriberId ASC)

                EXEC sp_fulltext_table N'dbo.v_SubscriberSearch', N'create', N'CCSearchCat', N'UCI_SearchView'
                GO

                EXEC sp_fulltext_column N'dbo.v_SubscriberSearch', N'SearchString', N'add', 0 /* neutral */
                GO

                --Activate full-text for table/view
                EXEC sp_fulltext_table N'dbo.v_SubscriberSearch', N'activate'
                GO

                --Full-text index update
                exec sp_fulltext_catalog 'CCSearchCat', 'start_full'
                GO
            ", true);

            migrationBuilder.Sql(@"
                EXEC('


                    /*
                    <remarks>
                    2019.03.14 - Bill Koenig - Created
                    2019.03.20 - Brent Ferree - modified to use full text indexing/ contains
                    </remarks>
                    <description>
                    Performs a LIKE search across a variety of related subscriber properties with an optional filter on the referral of the subscriber (how did they sign-up). 
                    Future improvements if the search remains in SQL: 
                        - Allow the filter to support more than one field, or create separate parameters for different types of filters
                        - Reduce the size of the fields that are used by the search query so that they can be indexed for better performance
                        - Refactor to eliminate duplication of effort, keeping in mind SQL limitations with short-circuit logic
                    </description>
                    <example>
                    EXEC [dbo].[System_Search_Subscribers] @Filter = NULL, @Query = ''C#''
                    </example>
                    */
                    alter PROCEDURE [dbo].[System_Search_Subscribers] (
                        @Filter NVARCHAR(100) = NULL,
                        @Query NVARCHAR(500) = NULL
                    )
                    AS
                    BEGIN
                    Declare @Query1 NVARCHAR(500) = null

                        -- cannot rely on short-circuiting, so create two different paths for evaluation
                        IF NULLIF(@Query, '''') IS NULL
                        BEGIN
                            -- simple search
                            ;WITH subscriberSources AS (
                                SELECT s.SubscriberId
                                    , ISNULL(JSON_VALUE(ProfileData, ''$.source''), ''None'') [Source]
                                    , ISNULL(JSON_VALUE(ProfileData, ''$.referer''), ''None'') [Referrer]
                                FROM Subscriber s WITH(NOLOCK)
                                LEFT JOIN SubscriberProfileStagingStore spss WITH(NOLOCK) ON s.SubscriberId = spss.SubscriberId 
                                    -- placing this logic in the join intentionally so that subscribers without sources are included in the result set
                                    AND ProfileSource = ''CareerCircle'' 
                                    AND ProfileFormat = ''Json''
                                    AND spss.IsDeleted = 0
                                WHERE s.IsDeleted = 0)
                            SELECT DISTINCT s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.City, st.Name [State], s.ModifyDate
                            FROM Subscriber s WITH(NOLOCK)
                            LEFT JOIN [State] st WITH(NOLOCK) ON s.StateId = st.StateId
                            INNER JOIN subscriberSources cte WITH(NOLOCK) ON s.SubscriberId = cte.SubscriberId
                            LEFT JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
                            LEFT JOIN Skill k WITH(NOLOCK) ON k.SkillId = ss.SkillId
                            LEFT JOIN SubscriberWorkHistory wh WITH(NOLOCK) ON s.SubscriberId = wh.SubscriberId
                            LEFT JOIN Company c WITH(NOLOCK) ON wh.CompanyId = c.CompanyId
                            LEFT JOIN SubscriberEducationHistory seh WITH(NOLOCK) ON s.SubscriberId = seh.SubscriberId
                            LEFT JOIN EducationalDegree ed WITH(NOLOCK) ON seh.EducationalDegreeId = ed.EducationalDegreeId
                            LEFT JOIN EducationalDegreeType edt WITH(NOLOCK) ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
                            LEFT JOIN EducationalInstitution ei WITH(NOLOCK) ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
                            WHERE cte.Referrer = @Filter OR NULLIF(@Filter, '''') IS NULL
                            OPTION(RECOMPILE);
                        END
                        ELSE
                        BEGIN
                            -- complex search
                            SET @Query1 =  ''''''''+ @Query +''''''''

                            SET @Query = ''""'' + @Query + '' * ""''


                            ; WITH subscriberSources AS(
                                 SELECT s.SubscriberId
                                     , ISNULL(JSON_VALUE(ProfileData, ''$.source''), ''None'')[Source]
                                     , ISNULL(JSON_VALUE(ProfileData, ''$.referer''), ''None'')[Referrer]
 
                                 FROM Subscriber s WITH(NOLOCK)
 
                                 LEFT JOIN SubscriberProfileStagingStore spss WITH(NOLOCK) ON s.SubscriberId = spss.SubscriberId
                                     -- placing this logic in the join intentionally so that subscribers without sources are included in the result set
 
                                     AND ProfileSource = ''CareerCircle''
 
                                     AND ProfileFormat = ''Json''
 
                                     AND spss.IsDeleted = 0
 
                                 WHERE s.IsDeleted = 0)
                            SELECT DISTINCT s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.City, st.Name[State], s.ModifyDate
                           FROM Subscriber s WITH(NOLOCK)
                            LEFT JOIN[State] st WITH(NOLOCK) ON s.StateId = st.StateId
                            INNER JOIN subscriberSources cte WITH(NOLOCK) ON s.SubscriberId = cte.SubscriberId
                            LEFT JOIN SubscriberSkill ss WITH(NOLOCK) ON s.SubscriberId = ss.SubscriberId
                            LEFT JOIN Skill k WITH(NOLOCK) ON k.SkillId = ss.SkillId
                            LEFT JOIN SubscriberWorkHistory wh WITH(NOLOCK) ON s.SubscriberId = wh.SubscriberId
                            LEFT JOIN Company c WITH(NOLOCK) ON wh.CompanyId = c.CompanyId
                            LEFT JOIN SubscriberEducationHistory seh WITH(NOLOCK) ON s.SubscriberId = seh.SubscriberId
                            LEFT JOIN EducationalDegree ed WITH(NOLOCK) ON seh.EducationalDegreeId = ed.EducationalDegreeId
                            LEFT JOIN EducationalDegreeType edt WITH(NOLOCK) ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
                            LEFT JOIN EducationalInstitution ei WITH(NOLOCK) ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
                            left join v_SubscriberSearch vss WITH(NOLOCK) on s.SubscriberId = vss.SubscriberId
                            WHERE(cte.Referrer = @Filter OR NULLIF(@Filter, '''') IS NULL)
                            AND(
                                   contains(vss.*, @Query1)
                                OR contains(k.SkillName, @Query)
                                OR contains(wh.JobDecription, @Query)
                                OR contains(wh.Title, @Query)
                                OR contains(c.CompanyName, @Query)
                                OR contains(ed.Degree, @Query)
                                OR contains(edt.DegreeType, @Query)
                                OR contains(ei.[Name], @Query)
                            )
                            OPTION(RECOMPILE);
                            END
                        END
                ')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
