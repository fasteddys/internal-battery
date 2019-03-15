using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class sprocSystem_Search_Subscribers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.03.14 - Bill Koenig - Created
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
CREATE PROCEDURE [dbo].[System_Search_Subscribers] (
	@Filter NVARCHAR(100) = NULL,
	@Query NVARCHAR(500) = NULL
)
AS
BEGIN

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
		SET @Query = ''%'' + @Query + ''%''

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
		WHERE (cte.Referrer = @Filter OR NULLIF(@Filter, '''') IS NULL)
		AND (
			s.LastName LIKE @Query
			OR s.FirstName LIKE @Query
			OR s.PhoneNumber LIKE @Query
			OR s.[Address] LIKE @Query
			OR s.City LIKE @Query
			OR s.Email LIKE @Query
			OR k.SkillName LIKE @Query
			OR wh.JobDecription LIKE @Query
			OR wh.Title LIKE @Query
			OR c.CompanyName LIKE @Query
			OR ed.Degree LIKE @Query
			OR edt.DegreeType LIKE @Query
			OR ei.[Name] LIKE @Query
		)
		OPTION(RECOMPILE);
	END
END
')
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Search_Subscribers]");
        }
    }
}
