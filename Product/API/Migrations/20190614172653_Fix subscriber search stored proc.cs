using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Fixsubscribersearchstoredproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC('
                    /*
    <remarks>
    2019.03.14 - Bill Koenig - Created
    2019.03.20 - Brent Ferree - modified to use full text indexing/ contains
	2019.05.03 - JAB added createdate to the resultset
	2019.06.14 = JAB fixed typo in JobDescription column name 
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
    ALTER PROCEDURE [dbo].[System_Search_Subscribers] (
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
            SELECT DISTINCT s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.City, st.Name [State], s.ModifyDate, s.CreateDate
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
            SELECT DISTINCT s.SubscriberGuid, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.City, st.Name[State], s.ModifyDate, s.CreateDate
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
                OR contains(wh.JobDescription, @Query)
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
