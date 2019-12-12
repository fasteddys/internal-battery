using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class jobsitemaprefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019-12-12 - Bill Koenig - Created
</remarks>
<description>
Replaces all non-alphanumeric characters with a hyphen and forces the output value to be lower case.
</description>
<example>
SELECT [dbo].[fn_SanitizeUrl](''Lee''''s Summit'')
</example>
*/
CREATE FUNCTION [dbo].[fn_SanitizeUrl]
(
	@UrlToSanitize VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	
	DECLARE @ReplacementValue VARCHAR(1) = ''-''
    DECLARE @KeepValues VARCHAR(15) = ''%[^a-z0-9\-]%''

    WHILE PATINDEX(@KeepValues, @UrlToSanitize) > 0
        SET @UrlToSanitize = STUFF(@UrlToSanitize, PATINDEX(@KeepValues, @UrlToSanitize), 1, ISNULL(@ReplacementValue, ''''))

    RETURN LOWER(@UrlToSanitize)
END    
            ')");

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.12 - Bill Koenig - Created
</remarks>
<description>
Returns all browse by location and job detail urls for the sitemap. Per Brent and Jon, browse by industry and category do not need to be supported at this time. Url values are 
forced to lower case and sanitized so as to remove any non-alphanumeric characters and replace them with hyphens. Results are sorted to return the broadest level of 
categorization first with the detailed job urls last. Note that recursive common table expressions are used for page numbering (e.g. countryWithPageNumbers).
</description>
<example>
EXEC [dbo].[System_Get_JobSitemapUrls] @BaseSiteUrl = ''https://www.careercircle.com''
</example>
*/
CREATE PROCEDURE [dbo].[System_Get_JobSitemapUrls] (
    @BaseSiteUrl VARCHAR(500)
)
AS
BEGIN

    SET NOCOUNT ON;

    SET @BaseSiteUrl = TRIM(''/'' FROM @BaseSiteUrl)

	;WITH sanitizedJobPostingData AS (
		SELECT ISNULL([dbo].[fn_SanitizeUrl](i.[Name]), ''all'') [Industry], ISNULL([dbo].[fn_SanitizeUrl](jc.[Name]), ''all'') [Category], ISNULL([dbo].[fn_SanitizeUrl](Country), ''all'') [Country], ISNULL([dbo].[fn_SanitizeUrl](Province), ''all'') [Province], ISNULL([dbo].[fn_SanitizeUrl](City), ''all'') [City], JobPostingGuid
		FROM JobPosting j
		LEFT JOIN JobCategory jc ON j.JobCategoryId = jc.JobCategoryId
		LEFT JOIN Industry i ON j.IndustryId = i.IndustryId
		WHERE j.IsDeleted = 0
	), countryPageCount AS (
		SELECT Country, CEILING(CAST(COUNT(1) AS FLOAT) / CAST(10 AS FLOAT)) [PageCount]
		FROM sanitizedJobPostingData
		WHERE Country <> ''all''
		GROUP BY Country
	), countryWithPageNumbers AS (
		SELECT Country, [PageCount] [PageNumber]
		FROM countryPageCount
		UNION ALL
		SELECT Country, [PageNumber] - 1 
		FROM countryWithPageNumbers
		WHERE [PageNumber] > 1
	), browseByCountryUrls AS (
		SELECT PageNumber, Country, NULL AS Province, NULL AS City, @BaseSiteUrl + ''/browse-jobs-location/'' + Country + ''/'' + CONVERT(VARCHAR(5), [PageNumber]) [url]
		FROM countryWithPageNumbers
	), provincePageCount AS (
		SELECT Country, Province, CEILING(CAST(COUNT(1) AS FLOAT) / CAST(10 AS FLOAT)) [PageCount]
		FROM sanitizedJobPostingData
		WHERE Country <> ''all''
		AND Province <> ''all''
		GROUP BY Country, Province
	), provinceWithPageNumbers AS (
		SELECT Country, Province, [PageCount] [PageNumber]
		FROM provincePageCount
		UNION ALL
		SELECT Country, Province, [PageNumber] - 1 
		FROM provinceWithPageNumbers
		WHERE [PageNumber] > 1
	), browseByStateUrls AS (
		SELECT PageNumber, Country, Province, NULL AS City, @BaseSiteUrl + ''/browse-jobs-location/'' + Country + ''/'' + + Province + ''/'' + CONVERT(VARCHAR(5), [PageNumber]) [url]
		FROM provinceWithPageNumbers
	), cityPageCount AS (
		SELECT Country, Province, City, CEILING(CAST(COUNT(1) AS FLOAT) / CAST(10 AS FLOAT)) [PageCount]
		FROM sanitizedJobPostingData
		WHERE Country <> ''all''
		AND Province <> ''all''
		AND City <> ''all''
		GROUP BY Country, Province, City
	), cityWithPageNumbers AS (
		SELECT Country, Province, City, [PageCount] [PageNumber]
		FROM cityPageCount
		UNION ALL
		SELECT Country, Province, City, [PageNumber] - 1 
		FROM cityWithPageNumbers
		WHERE [PageNumber] > 1
	), browseByCityUrls AS (
		SELECT PageNumber, Country, Province, City, @BaseSiteUrl + ''/browse-jobs-location/'' + Country + ''/'' + + Province + ''/'' + City + ''/'' + CONVERT(VARCHAR(5), [PageNumber]) [url]
		FROM cityWithPageNumbers
	), jobDetailUrls AS (
		SELECT NULL PageNumber, NULL Country, NULL Province, NULL City, @BaseSiteUrl + ''/job/'' + Industry + ''/'' + Category + ''/'' + Country + ''/'' + Province + ''/'' + City + ''/'' + LOWER(JobPostingGuid) [url]
		FROM sanitizedJobPostingData
	), allJobUrls AS (
		SELECT *
		FROM browseByCountryUrls
		UNION ALL
		SELECT * 
		FROM browseByStateUrls
		UNION ALL
		SELECT *
		FROM browseByCityUrls
		UNION ALL
		SELECT *
		FROM jobDetailUrls
	)
	SELECT [url]
	FROM allJobUrls
	ORDER BY CASE WHEN PageNumber IS NULL THEN 1 ELSE 0 END, Country ASC, Province ASC, City ASC, PageNumber ASC 
	OPTION (MAXRECURSION 10000)
END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Get_JobSitemapUrls]");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[fn_SanitizeUrl]");
        }
    }
}
