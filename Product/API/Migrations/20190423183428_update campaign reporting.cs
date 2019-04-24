using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatecampaignreporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
2019.04.23 - Bill Koenig - Excluding test leads from reporting. Tried to remove redundant statements, but could not get it to perform as well.
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignStatistics] 
AS
BEGIN

SELECT [CampaignGuid]
	, [name]
	, startDate
	, endDate
	, (SELECT COUNT(*) FROM CampaignPartnerContact cpc INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId AND cpc.CampaignId = c.CampaignId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0) AS EmailsSent
	, (SELECT COUNT(*) FROM PartnerContactAction pca INNER JOIN PartnerContact pc ON pca.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0 WHERE ActionId = 1 AND pca.CampaignId = c.CampaignId) AS OpenEmail
	, (SELECT COUNT(*) FROM PartnerContactAction pca INNER JOIN PartnerContact pc ON pca.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0 WHERE ActionId = 2 AND pca.CampaignId = c.CampaignId) AS VisitLandingPage
	, (SELECT COUNT(*) FROM PartnerContactAction pca INNER JOIN PartnerContact pc ON pca.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0 WHERE ActionId = 3 AND pca.CampaignId = c.CampaignId) AS CreateAcount
	, (SELECT COUNT(*) FROM PartnerContactAction pca INNER JOIN PartnerContact pc ON pca.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0 WHERE ActionId = 4 AND pca.CampaignId = c.CampaignId) AS CourseEnrollment
	, (SELECT COUNT(*) FROM PartnerContactAction pca INNER JOIN PartnerContact pc ON pca.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' THEN 1 ELSE 0 END, 0) = 0 WHERE ActionId = 5 AND pca.CampaignId = c.CampaignId) AS CourseCompletion
FROM Campaign c 
ORDER BY StartDate DESC
	 
END
            ')");

            migrationBuilder.Sql(@"EXEC('
/*     
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.03.12 - Bill Koenig - Modified logic to return campaigns which do not have course offers, exclude logical deletes, ordered output, added example to comment block
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
2019.04.23 - Bill Koenig - Excluding test leads from reporting
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
EXEC [dbo].[System_CampaignDetails] @CampaignGuid = ''AC966B23-B18F-45B1-BE02-07BD475CC6C2''
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
    @CampaignGuid UNIQUEIDENTIFIER 
AS
BEGIN

    SELECT 
		c.Name CourseName, 
		rt.Name RebateType, 
		co.Email, 
		JSON_VALUE(pc.MetaDataJSON, ''$.FirstName'') [FirstName], 
		JSON_VALUE(pc.MetaDataJSON, ''$.LastName'') [LastName], 
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 1 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as OpenEmail,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 2 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as VisitLandingPage,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 3 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CreateAcount,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 4 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseEnrollment,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 5 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseCompletion
    FROM Campaign cp
		INNER JOIN CampaignPartnerContact cpc ON cp.CampaignId = cpc.CampaignId
		INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId AND ISNULL(CASE WHEN JSON_VALUE(pc.MetaDataJSON, ''$.IsTest'') = ''true'' then 1 else 0 end, 0) = 0
		INNER JOIN Contact co ON pc.ContactId = co.ContactId
		LEFT JOIN CampaignCourseVariant ccv ON ccv.CampaignId = cp.CampaignId
		LEFT JOIN CourseVariant cv ON ccv.CourseVariantId = cv.CourseVariantId
		LEFT JOIN Course c ON cv.CourseId = c.CourseId
		LEFT JOIN RebateType rt ON ccv.RebateTypeId = rt.RebateTypeId
    WHERE cp.CampaignGuid = @CampaignGuid 
		AND co.IsDeleted = 0
		AND cp.IsDeleted = 0
		AND pc.IsDeleted = 0
		AND co.IsDeleted = 0
	ORDER BY OpenEmail DESC, VisitLandingPage DESC, CreateAcount DESC, CourseEnrollment DESC, CourseCompletion DESC

END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignStatistics] 
AS
BEGIN

 select [CampaignGuid],[name],startDate,endDate,
(select count(*) from CampaignPartnerContact cpc where  cpc.CampaignId = c.CampaignId) as EmailsSent,
(select count(*) from PartnerContactAction pca where ActionId = 1 and pca.CampaignId = c.CampaignId) as OpenEmail,
(select count(*) from PartnerContactAction pca where ActionId = 2 and pca.CampaignId = c.CampaignId) as VisitLandingPage,
(select count(*) from PartnerContactAction pca where ActionId = 3 and pca.CampaignId = c.CampaignId) as CreateAcount,
(select count(*) from PartnerContactAction pca where ActionId = 4 and pca.CampaignId = c.CampaignId) as CourseEnrollment,
(select count(*) from PartnerContactAction pca where ActionId = 5 and pca.CampaignId = c.CampaignId) as CourseCompletion
 from campaign as c order by StartDate desc ;
	 
END
')
            ");

            migrationBuilder.Sql(@"EXEC('

/*
<remarks>
2019.02.13 - Jim Brazil - Created 
2019.03.12 - Bill Koenig - Modified logic to return campaigns which do not have course offers, exclude logical deletes, ordered output, added example to comment block
2019.04.10 - Bill Koenig - Replaced ContactAction with PartnerContactAction
</remarks>
<description>
Returns statistics for marketing campaigns 
</description>
<example>
EXEC [dbo].[System_CampaignDetails] @CampaignGuid = ''AC966B23-B18F-45B1-BE02-07BD475CC6C2''
</example>
*/
ALTER PROCEDURE [dbo].[System_CampaignDetails]
    @CampaignGuid UNIQUEIDENTIFIER 
AS
BEGIN

    SELECT 
		c.Name CourseName, 
		rt.Name RebateType, 
		co.Email, 
		JSON_VALUE(pc.MetaDataJSON, ''$.FirstName'') [FirstName], 
		JSON_VALUE(pc.MetaDataJSON, ''$.LastName'') [LastName], 
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 1 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as OpenEmail,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 2 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as VisitLandingPage,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 3 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CreateAcount,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 4 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseEnrollment,
		(SELECT COUNT(*) FROM PartnerContactAction pca WHERE ActionId = 5 AND pca.CampaignId = cp.CampaignId AND pca.PartnerContactId = pc.PartnerContactId) as CourseCompletion
    FROM Campaign cp
		INNER JOIN CampaignPartnerContact cpc ON cp.CampaignId = cpc.CampaignId
		INNER JOIN PartnerContact pc ON cpc.PartnerContactId = pc.PartnerContactId
		INNER JOIN Contact co ON pc.ContactId = co.ContactId
		LEFT JOIN CampaignCourseVariant ccv ON ccv.CampaignId = cp.CampaignId
		LEFT JOIN CourseVariant cv ON ccv.CourseVariantId = cv.CourseVariantId
		LEFT JOIN Course c ON cv.CourseId = c.CourseId
		LEFT JOIN RebateType rt ON ccv.RebateTypeId = rt.RebateTypeId
    WHERE cp.CampaignGuid = @CampaignGuid 
		AND co.IsDeleted = 0
		AND cp.IsDeleted = 0
		AND pc.IsDeleted = 0
		AND co.IsDeleted = 0
	ORDER BY OpenEmail DESC, VisitLandingPage DESC, CreateAcount DESC, CourseEnrollment DESC, CourseCompletion DESC

END
')
            ");
        }
    }
}
