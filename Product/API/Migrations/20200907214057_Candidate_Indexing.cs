using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Candidate_Indexing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-07-24 - JAB - Created
2020-07-30 - JAB - Added support for work history
2020-08-04 - JAB - Updated to get subscriber current rate from 360 info on subscriber record
2020-08-05 - JAB - Updated to use calculation for current rate
2020-08-11 - Bill Koenig - Correcting STRING_AGG error (exceeded the limit of 8000 bytes)
s0s0-08-25 - Jab - Changes data delimeters to non-printable characters => 1D Group Separator and 1E Record Separator
2020-09-09 - Joey Herrington - Added additional fields to be indexed
</remarks>
<description>
Returns Candidate data from multiple sources in a flattened format for indexing in Azure Search.
</description>
<example>
SELECT * FROM [B2B].[v_CandidateAzureSearch]
</example>
*/
ALTER VIEW [B2B].[v_CandidateAzureSearch]
AS
    WITH subscriberSkills AS (
        SELECT s.SubscriberId, STRING_AGG(CAST(sk.SkillName AS NVARCHAR(MAX)), CHAR(30)) Skills
        FROM dbo.Subscriber s
        INNER JOIN dbo.SubscriberSkill ss ON s.SubscriberId = ss.SubscriberId
        INNER JOIN dbo.Skill sk ON ss.SkillId = sk.SkillId
        WHERE s.IsDeleted = 0 AND ss.IsDeleted = 0
        GROUP BY s.SubscriberId),
    subscriberLanguages AS (
        SELECT s.SubscriberId, STRING_AGG(CAST(l.LanguageName AS NVARCHAR(MAX)) + CHAR(29) + pl.ProficiencyLevelName, CHAR(30)) SubscriberLanguages
        FROM dbo.Subscriber s
        INNER JOIN dbo.SubscriberLanguageProficiencies slp ON s.SubscriberId = slp.SubscriberId
        INNER JOIN dbo.ProficiencyLevels pl ON slp.ProficiencyLevelId = pl.ProficiencyLevelId
        INNER JOIN dbo.Languages l on slp.LanguageId = l.LanguageId
        WHERE s.IsDeleted = 0 AND slp.IsDeleted = 0 
        GROUP BY s.SubscriberId),
    employmentTypesCollection AS (
        SELECT se.SubscriberId, et.[Name]
        FROM dbo.SubscriberEmploymentTypes se
        INNER JOIN dbo.EmploymentType et ON se.EmploymentTypeId = et.EmploymentTypeId
        WHERE se.IsDeleted = 0 AND et.IsDeleted = 0
        UNION SELECT SubscriberId, [Name]
        FROM (
            SELECT SubscriberId, [Name], [Value]
            FROM dbo.Subscriber
            UNPIVOT ([Value] FOR [Name] IN (IsWillingToTravel, IsFlexibleWorkScheduleRequired)) s
        ) pivoted
        WHERE [Value] = 1
    ),
    subscriberEmploymentTypes AS (
        SELECT s.SubscriberId, STRING_AGG(et.[Name], CHAR(30)) EmploymentTypes
        FROM dbo.Subscriber s
        INNER JOIN employmentTypesCollection et ON s.SubscriberId = et.SubscriberId
        WHERE s.IsDeleted = 0
        GROUP BY s.SubscriberId),
    subscriberTrainings AS (
        SELECT s.SubscriberId, STRING_AGG( tt.[Name] + CHAR(29) + st.TrainingInstitution + CHAR(29) + st.TrainingName, CHAR(30)) SubscriberTraining
        FROM dbo.Subscriber s
        INNER JOIN dbo.SubscriberTraining st ON s.SubscriberId = st.SubscriberId
        INNER JOIN dbo.TrainingType tt ON st.TrainingTypeId = tt.TrainingTypeId 
        WHERE s.IsDeleted = 0 AND st.IsDeleted = 0 
        GROUP BY s.SubscriberId),
    subscriberEducation AS (
        SELECT s.SubscriberId, STRING_AGG(ISNULL(ei.[Name],'''') + CHAR(29) + ISNULL(edt.DegreeType,'''') + CHAR(29) + ISNULL(ed.Degree,''''), CHAR(30)) SubscriberEducation
        FROM dbo.Subscriber s
        INNER JOIN dbo.SubscriberEducationHistory seh ON s.SubscriberId = seh.SubscriberId
        INNER JOIN dbo.EducationalInstitution ei ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
        INNER JOIN dbo.EducationalDegree ed ON seh.EducationalDegreeId = ed.EducationalDegreeId
        INNER JOIN dbo.EducationalDegreeType edt ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
        WHERE s.IsDeleted = 0 AND seh.IsDeleted = 0
        GROUP BY s.SubscriberId),
    subscriberWorkHistories AS (
        SELECT s.SubscriberId, STRING_AGG(ISNULL(c.CompanyName,'''') + CHAR(29) + ISNULL(swh.Title,''''), CHAR(30)) SubscriberWorkHistory
        FROM dbo.Subscriber s
        INNER JOIN dbo.SubscriberWorkHistory swh ON s.SubscriberId = swh.SubscriberId
        INNER JOIN dbo.Company c ON swh.CompanyId = c.CompanyId
        WHERE s.IsDeleted = 0 AND swh.IsDeleted = 0 
        GROUP BY s.SubscriberId),
    profileComments AS (
        SELECT p.SubscriberId, ISNULL(c.ModifyDate, c.CreateDate) ContactDate, ROW_NUMBER() OVER (PARTITION BY p.SubscriberId ORDER BY ISNULL(c.ModifyDate, c.CreateDate) DESC) rownum
        FROM G2.Profiles p 
        INNER JOIN G2.ProfileComments c ON p.ProfileId = c.ProfileId
        WHERE p.IsDeleted = 0 AND c.IsDeleted = 0),
    subscriberVideos AS (
        SELECT v.SubscriberId, v.VideoLink, v.ThumbnailLink, ROW_NUMBER() OVER (PARTITION BY s.SubscriberId ORDER BY v.CreateDate DESC) rownum
        FROM dbo.Subscriber s 
        INNER JOIN dbo.SubscriberVideos v ON s.SubscriberId = v.SubscriberId
        WHERE s.IsDeleted = 0 AND s.IsVideoVisibleToHiringManager = 1 AND v.IsDeleted = 0 AND v.IsPublished = 1),
    traitifyResults AS (
        SELECT s.SubscriberId, REPLACE(JSON_VALUE(ResultData, ''$.personality_blend.name''), ''/'', CHAR(30)) Personalities, JSON_VALUE(ResultData, ''$.personality_blend.personality_type_1.badge.image_small'') Personality1ImageUrl, JSON_VALUE(ResultData, ''$.personality_blend.personality_type_2.badge.image_small'') Personality2ImageUrl, JSON_VALUE(ResultData, ''$.personality_blend.name'') PersonalityBlendName, ROW_NUMBER() OVER (PARTITION BY s.SubscriberId ORDER BY t.CreateDate DESC) rownum
        FROM dbo.Subscriber s 
        INNER JOIN dbo.Traitify t ON s.SubscriberId = t.SubscriberId
        WHERE s.IsDeleted = 0 AND t.IsDeleted = 0 AND s.IsTraitifyAssessmentsVisibleToHiringManagers = 1),
    certificationDates AS (
        SELECT s.SubscriberId, ISNULL(st.ModifyDate, st.CreateDate) CertificationDate, ROW_NUMBER() OVER (PARTITION BY s.SubscriberId ORDER BY ISNULL(st.ModifyDate, st.CreateDate) DESC) rownum
        FROM dbo.Subscriber s
        INNER JOIN SubscriberTraining st ON s.SubscriberId = st.SubscriberId
        WHERE s.IsDeleted = 0 AND st.IsDeleted = 0),
    resumes AS (
        SELECT s.SubscriberId, sf.SubscriberFileId, ROW_NUMBER() OVER (PARTITION BY s.SubscriberId ORDER BY sf.CreateDate DESC) rownum
        FROM dbo.Subscriber s
        INNER JOIN SubscriberFile sf ON s.SubscriberId = sf.SubscriberId
        WHERE s.IsDeleted = 0 AND sf.IsDeleted = 0)
    SELECT s.AvatarUrl
        , s.AzureIndexStatusId
        , CASE WHEN LEN(s.City) > 0 THEN s.City ELSE NULL END City
        , cd.DistanceRange AS CommuteDistance
        , s.CreateDate
        , CASE WHEN s.CurrentSalary IS NOT NULL THEN CAST(s.CurrentSalary AS FLOAT) ELSE (CASE WHEN s.CurrentRate IS NOT NULL THEN CAST(s.CurrentRate * 40 * 52 AS FLOAT) ELSE 0 END) END CurrentRate
        , CASE WHEN s.DesiredSalary IS NOT NULL THEN CAST(s.DesiredSalary AS FLOAT) ELSE (CASE WHEN s.DesiredRate IS NOT NULL THEN CAST(s.DesiredRate * 40 * 52 AS FLOAT) ELSE 0 END) END DesiredRate
        , se.SubscriberEducation Education
        , s.Email
        , CASE WHEN LEN(s.CoverLetter) > 140 THEN LEFT(s.CoverLetter, 140) + ''...'' ELSE s.CoverLetter END ExperienceSummary
        , CASE WHEN LEN(s.FirstName) > 0 THEN s.FirstName ELSE NULL END FirstName
        , CAST(CASE WHEN r.SubscriberId IS NOT NULL THEN 1 ELSE 0 END AS BIT) IsResumeUploaded
        , sl.SubscriberLanguages Languages
        , ct.CertificationDate LastCertifiedDate
        , pc.ContactDate LastContactDate
        , CASE WHEN LEN(s.LastName) > 0 THEN s.LastName ELSE NULL END LastName
        , dbo.fn_GetGeographyCoordinate((SELECT TOP 1 CityId FROM dbo.City c INNER JOIN dbo.[State] t ON c.StateId = t.StateId WHERE c.[Name] = s.City AND t.StateId = s.StateId), s.StateId, (SELECT TOP 1 PostalId FROM dbo.Postal WHERE Code = s.PostalCode)) [Location]
        , s.ModifyDate
        , tr.Personalities
        , tr.Personality1ImageUrl
        , tr.Personality2ImageUrl
        , tr.PersonalityBlendName
        , CASE WHEN LEN(s.PhoneNumber) > 0 THEN s.PhoneNumber ELSE NULL END PhoneNumber
        , CASE WHEN LEN(s.PostalCode) > 0 THEN s.PostalCode ELSE NULL END Postal
        , p.ProfileGuid
        , ss.Skills 
        , t.Code [State]
        , CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress
        , s.SubscriberGuid
        , sv.ThumbnailLink ThumbnailImageUrl
        , s.Title   
        , st.SubscriberTraining Training
        , sv.VideoLink VideoUrl
        , swh.SubscriberWorkHistory WorkHistories
        , et.EmploymentTypes WorkPreferences
    FROM Subscriber s   
        INNER JOIN G2.Profiles p ON s.SubscriberId = p.SubscriberId
        INNER JOIN dbo.Company c ON p.CompanyId = c.CompanyId AND c.CompanyName = ''CareerCircle''
        LEFT JOIN [State] t ON s.StateId = t.StateId
        LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId 
        LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId AND hm.IsDeleted = 0
        LEFT JOIN subscriberLanguages sl on s.SubscriberId = sl.SubscriberId
        LEFT JOIN CommuteDistance cd on cd.CommuteDistanceId = s.CommuteDistanceId
        LEFT JOIN subscriberEmploymentTypes et ON s.SubscriberId = et.SubscriberId 
        LEFT JOIN subscriberTrainings st ON s.SubscriberId = st.SubscriberId 
        LEFT JOIN subscriberEducation se ON s.SubscriberId = se.SubscriberId 
        LEFT JOIN subscriberWorkHistories swh ON s.SubscriberId = swh.SubscriberId
        LEFT JOIN subscriberVideos sv ON s.SubscriberId = sv.SubscriberId AND sv.rownum = 1
        LEFT JOIN traitifyResults tr ON s.SubscriberId = tr.SubscriberId AND tr.rownum = 1
        LEFT JOIN profileComments pc ON s.SubscriberId = pc.SubscriberId AND pc.rownum = 1
        LEFT JOIN certificationDates ct ON s.SubscriberId = ct.SubscriberId AND ct.rownum = 1
        LEFT JOIN resumes r ON s.SubscriberId = r.SubscriberId AND r.rownum = 1
    WHERE s.IsDeleted = 0
    AND hm.HiringManagerId IS NULL;')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020-07-24 - JAB - Created
2020-07-30 - JAB - Added support for work history
2020-08-04 - JAB - Updated to get subscriber current rate from 360 info on subscriber record
2020-08-05 - JAB - Updated to use calculation for current rate
2020-08-11 - Bill Koenig - Correcting STRING_AGG error (exceeded the limit of 8000 bytes)
s0s0-08-25 - Jab - Changes data delimeters to non-printable characters => 1D Group Separator and 1E Record Separator
</remarks>
<description>
Returns Candidate data from multiple sources in a flattened format for indexing in Azure Search.
</description>
<example>
SELECT * FROM [B2B].[v_CandidateAzureSearch]
</example>
*/
ALTER VIEW [B2B].[v_CandidateAzureSearch]
AS
    WITH subscriberWorkHistoryWithRownum AS (
        -- returns all subscriber work history with compensation data if it is hourly
        SELECT swh.SubscriberId, swh.StartDate, swh.Title, swh.Compensation, ct.CompensationTypeName, ISNULL(swh.ModifyDate, swh.CreateDate) ModifyDate, ROW_NUMBER() OVER(PARTITION BY swh.SubscriberId ORDER BY swh.StartDate DESC) rownum
        FROM SubscriberWorkHistory swh
        LEFT JOIN CompensationType ct ON swh.CompensationTypeId = ct.CompensationTypeId
        WHERE swh.IsDeleted = 0
    ), mostRecentSubscriberWorkHistory AS (
        -- uses the previous CTE to find the most recent work history record to use for title and compensation in subscriber curated data
        SELECT SubscriberId, Title, Compensation, CompensationTypeName, ModifyDate
        FROM subscriberWorkHistoryWithRownum
        WHERE rownum = 1
    ), subscriberSkills AS (
               
        SELECT s.SubscriberId, STRING_AGG(CAST(sk.SkillName AS NVARCHAR(MAX)), char(30)) Skills, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(ss.ModifyDate), default, default, default) ModifyDate
        FROM Subscriber s
        LEFT JOIN SubscriberSkill ss ON s.SubscriberId = ss.SubscriberId
        LEFT JOIN dbo.Skill sk ON ss.SkillId = sk.SkillId
        WHERE s.IsDeleted = 0 and sk.IsDeleted = 0 and ss.IsDeleted = 0
        GROUP BY s.SubscriberId
        		
    ), 
        subscriberLanguages AS (			
        SELECT s.SubscriberId, STRING_AGG(CAST(l.LanguageName AS NVARCHAR(MAX)) + char(29) + pl.ProficiencyLevelName, char(30)) SubscriberLanguages, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(slp.ModifyDate), default, default, default) ModifyDate
        FROM Subscriber s
        LEFT JOIN SubscriberLanguageProficiencies slp ON s.SubscriberId = slp.SubscriberId
        LEFT JOIN ProficiencyLevels pl ON slp.ProficiencyLevelId = pl.ProficiencyLevelId
        LEFT JOIN Languages l on slp.LanguageId = l.LanguageId
        WHERE s.IsDeleted = 0 and slp.IsDeleted = 0 and pl.IsDeleted = 0 and l.IsDeleted = 0
        GROUP BY s.SubscriberId			 
        ), 
        subscriberEmploymentType AS (
        	
        SELECT s.SubscriberId, STRING_AGG(et.Name, char(30)) EmploymentTypes
        FROM Subscriber s
        LEFT JOIN SubscriberEmploymentTypes e ON s.SubscriberId = e.SubscriberId
        LEFT JOIN dbo.EmploymentType et ON e.EmploymentTypeId = et.EmploymentTypeId
        WHERE s.IsDeleted = 0 and e.IsDeleted = 0 and et.IsDeleted = 0
        GROUP BY s.SubscriberId
        			 
        ),
        subscriberTrainings AS (
        	
        	SELECT s.SubscriberId, STRING_AGG( tt.Name + char(29) + st.TrainingInstitution + char(29) + st.TrainingName, char(30)) SubscriberTraining
        FROM Subscriber s
        LEFT JOIN SubscriberTraining st ON s.SubscriberId = st.SubscriberId
        LEFT JOIN TrainingType tt ON st.TrainingTypeId = tt.TrainingTypeId 
        WHERE s.IsDeleted = 0 and st.IsDeleted = 0 and tt.IsDeleted = 0 
        GROUP BY s.SubscriberId	
        			 
        ),
        subscriberEducation AS (
        	 
        SELECT s.SubscriberId, STRING_AGG(ISNULL(ei.Name,'''') + char(29) +  ISNULL(edt.DegreeType,'''') + char(29) + ISNULL(ed.Degree,'''')  , char(30)) SubscriberEducation
        FROM Subscriber s
        LEFT JOIN  SubscriberEducationHistory seh ON s.SubscriberId = seh.SubscriberId
        LEFt JOIN EducationalInstitution ei ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
        LEFT JOIN EducationalDegree ed on seh.EducationalDegreeId = ed.EducationalDegreeId
        LEFT JOIN EducationalDegreeType edt ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
        WHERE s.IsDeleted = 0 and seh.IsDeleted = 0 
        GROUP BY s.SubscriberId	
        			 
        ),
        subscriberTitle AS (
        	 
        SELECT x.SubscriberId, STRING_AGG(ISNULL(x.Title,'''') , char(30)) SubscriberTitles From
        (SELECT s.subscriberId,swh.Title
        FROM Subscriber s
        LEFT JOIN  SubscriberWorkHistory swh ON s.SubscriberId = swh.SubscriberId    
        WHERE s.IsDeleted = 0 and swh.IsDeleted = 0 and swh.Title IS NOT NULL
        UNION SELECT subscriberid,title FROM Subscriber WHERE title  IS NOT NULL) X
        GROUP BY x.SubscriberId
        			 
        )	 	 	
        ,
        subscriberWorkHistories AS (
        	
        SELECT s.SubscriberId, STRING_AGG(ISNULL(c.CompanyName,'''') + char(29) +  ISNULL(swh.Title,'''')   , char(30)) SubscriberWorkHistory
        FROM Subscriber s
        LEFT JOIN  SubscriberWorkHistory swh ON s.SubscriberId = swh.SubscriberId    
        LEFT JOIN  Company c on swh.CompanyId = c.CompanyId
        WHERE s.IsDeleted = 0 and swh.IsDeleted = 0 
        GROUP BY s.SubscriberId	 
        			 
        )
        	 
        	         
        SELECT 
        		s.CreateDate
        	, s.SubscriberId
        	, s.SubscriberGuid
        	, CASE WHEN LEN(s.FirstName) > 0 THEN s.FirstName ELSE NULL END FirstName, CASE WHEN LEN(s.LastName) > 0 THEN s.LastName ELSE NULL END LastName
        	, s.Email
        	, CASE WHEN LEN(PhoneNumber) > 0 THEN PhoneNumber ELSE NULL END PhoneNumber
        	, CASE WHEN LEN([Address]) > 0 THEN [Address] ELSE NULL END StreetAddress, CASE WHEN LEN(City) > 0 THEN City ELSE NULL END City
        	, t.Code [State], CASE WHEN LEN(PostalCode) > 0 THEN PostalCode ELSE NULL END Postal
        	, mrswh.Title
        	, CASE WHEN s.CurrentSalary is not null 
    			    THEN CAST(s.CurrentSalary as float ) 
    				ELSE CASE WHEN s.CurrentRate is NOT NULL 
    					THEN  CAST(s.CurrentRate * 40 * 52  as float ) 
    					ELSE 0 
    					END 
    			END  CurrentRate 
        	, ss.Skills 
            , dbo.fn_GetGeographyCoordinate((SELECT TOP 1 CityId FROM dbo.City c INNER JOIN dbo.[State] t ON c.StateId = t.StateId WHERE c.[Name] = s.City AND t.StateId = s.StateId)
        	, s.StateId, (SELECT TOP 1 PostalId FROM dbo.Postal WHERE Code = s.PostalCode)) [Location]
        	, s.ModifyDate
        	, s.AvatarUrl
        	, ssd.PartnerGuid as PartnerGuid
        	, sl.SubscriberLanguages
        	, cd.DistanceRange as CommuteDistance
        	,s.IsWillingToTravel
        	,s.IsFlexibleWorkScheduleRequired
        	,et.EmploymentTypes
        	,st.SubscriberTraining
        	,se.SubscriberEducation
        	,stl.SubscriberTitles
        	,swh.SubscriberWorkHistory,
        	s.AzureIndexStatusId
        		   
         
        FROM Subscriber s	
        LEFT JOIN [State] t ON s.StateId = t.StateId
        LEFT JOIN mostRecentSubscriberWorkHistory mrswh ON s.SubscriberId = mrswh.SubscriberId
        LEFT JOIN subscriberSkills ss ON s.SubscriberId = ss.SubscriberId 
        LEFT JOIN v_SubscriberSourceDetails ssd on ssd.SubscriberId = s.SubscriberId and ssd.GroupRank = 1 and ssd.PartnerRank = 1
        LEFT JOIN B2B.HiringManagers hm ON s.SubscriberId = hm.SubscriberId
        LEFT JOIN subscriberLanguages sl on s.SubscriberId = sl.SubscriberId
        LEFT JOIN CommuteDistance cd on cd.CommuteDistanceId = s.CommuteDistanceId
        LEFT JOIN subscriberEmploymentType et ON s.SubscriberId = et.SubscriberId 
        LEFT JOIN subscriberTrainings st ON s.SubscriberId = st.SubscriberId 
        LEFT JOIN subscriberEducation se ON s.SubscriberId = se.SubscriberId 
        LEFT JOIN subscriberTitle stl ON s.SubscriberId = stl.SubscriberId 
        LEFT JOIN subscriberWorkHistories swh  ON s.SubscriberId = swh.SubscriberId 
        WHERE s.IsDeleted = 0
        AND hm.HiringManagerId IS NULL')");
        }
    }
}
