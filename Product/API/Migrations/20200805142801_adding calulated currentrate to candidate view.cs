using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class addingcalulatedcurrentratetocandidateview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('/*
    <remarks>
    2020-07-24 - JAB - Created
    2020-07-30 - JAB - Added support for work history
    2020-08-04 - JAB - Updated to get subscriber current rate from 360 info on subscriber record
	2020-08-05 - JAB - Updated to use calculation for current rate
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
           
    	    SELECT s.SubscriberId, STRING_AGG(sk.SkillName, '';'') Skills, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(ss.ModifyDate), default, default, default) ModifyDate
            FROM Subscriber s
            LEFT JOIN SubscriberSkill ss ON s.SubscriberId = ss.SubscriberId
            LEFT JOIN dbo.Skill sk ON ss.SkillId = sk.SkillId
            WHERE s.IsDeleted = 0 and sk.IsDeleted = 0 and ss.IsDeleted = 0
            GROUP BY s.SubscriberId
    		
        ), 
    	 subscriberLanguages AS (			
     	    SELECT s.SubscriberId, STRING_AGG( l.LanguageName + ''|'' + pl.ProficiencyLevelName, '';'') SubscriberLanguages, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(slp.ModifyDate), default, default, default) ModifyDate
            FROM Subscriber s
            LEFT JOIN SubscriberLanguageProficiencies slp ON s.SubscriberId = slp.SubscriberId
            LEFT JOIN ProficiencyLevels pl ON slp.ProficiencyLevelId = pl.ProficiencyLevelId
    		LEFT JOIN Languages l on slp.LanguageId = l.LanguageId
            WHERE s.IsDeleted = 0 and slp.IsDeleted = 0 and pl.IsDeleted = 0 and l.IsDeleted = 0
            GROUP BY s.SubscriberId			 
    	 ), 
    	 subscriberEmploymentType AS (
    	
    	 	SELECT s.SubscriberId, STRING_AGG(et.Name, '';'') EmploymentTypes
            FROM Subscriber s
            LEFT JOIN SubscriberEmploymentTypes e ON s.SubscriberId = e.SubscriberId
            LEFT JOIN dbo.EmploymentType et ON e.EmploymentTypeId = et.EmploymentTypeId
            WHERE s.IsDeleted = 0 and e.IsDeleted = 0 and et.IsDeleted = 0
            GROUP BY s.SubscriberId
    			 
    	 ),
    	  subscriberTrainings AS (
    	
    		 SELECT s.SubscriberId, STRING_AGG( tt.Name + ''|'' + st.TrainingInstitution + ''|'' + st.TrainingName, '';'') SubscriberTraining
            FROM Subscriber s
            LEFT JOIN SubscriberTraining st ON s.SubscriberId = st.SubscriberId
            LEFT JOIN TrainingType tt ON st.TrainingTypeId = tt.TrainingTypeId 
            WHERE s.IsDeleted = 0 and st.IsDeleted = 0 and tt.IsDeleted = 0 
            GROUP BY s.SubscriberId	
    			 
    	 ),
    	  subscriberEducation AS (
    	 
     		SELECT s.SubscriberId, STRING_AGG(ISNULL(ei.Name,'''') + ''|'' +  ISNULL(edt.DegreeType,'''') + ''|'' + ISNULL(ed.Degree,'''')  , '';'') SubscriberEducation
            FROM Subscriber s
            LEFT JOIN  SubscriberEducationHistory seh ON s.SubscriberId = seh.SubscriberId
            LEFt JOIN EducationalInstitution ei ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
    		LEFT JOIN EducationalDegree ed on seh.EducationalDegreeId = ed.EducationalDegreeId
    	    LEFT JOIN EducationalDegreeType edt ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
            WHERE s.IsDeleted = 0 and seh.IsDeleted = 0 
            GROUP BY s.SubscriberId	
    			 
    	 ),
    	 subscriberTitle AS (
    	 
     		SELECT x.SubscriberId, STRING_AGG(ISNULL(x.Title,'''') , '';'') SubscriberTitles From
    		(SELECT s.subscriberId,swh.Title
            FROM Subscriber s
            LEFT JOIN  SubscriberWorkHistory swh ON s.SubscriberId = swh.SubscriberId    
            WHERE s.IsDeleted = 0 and swh.IsDeleted = 0 and swh.Title IS NOT NULL
    		UNION SELECT subscriberid,title FROM Subscriber WHERE title  IS NOT NULL) X
    		GROUP BY x.SubscriberId
    			 
    	 )	 	 	
    	 ,
    	  subscriberWorkHistories AS (
    	
     		SELECT s.SubscriberId, STRING_AGG(ISNULL(c.CompanyName,'''') + ''|'' +  ISNULL(swh.Title,'''')   , '';'') SubscriberWorkHistory
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
    		AND hm.HiringManagerId IS NULL 
      ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Exec('/*
<remarks>
2020-07-24 - JAB - Created
2020-07-30 - JAB - Added support for work history
2020-08-04   JAB - Updated to get subscriber current rate from 360 info on subscriber record
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
       
	    SELECT s.SubscriberId, STRING_AGG(sk.SkillName, '';'') Skills, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(ss.ModifyDate), default, default, default) ModifyDate
        FROM Subscriber s
        LEFT JOIN SubscriberSkill ss ON s.SubscriberId = ss.SubscriberId
        LEFT JOIN dbo.Skill sk ON ss.SkillId = sk.SkillId
        WHERE s.IsDeleted = 0 and sk.IsDeleted = 0 and ss.IsDeleted = 0
        GROUP BY s.SubscriberId
		
    ), 
	 subscriberLanguages AS (			
 	    SELECT s.SubscriberId, STRING_AGG( l.LanguageName + ''|'' + pl.ProficiencyLevelName, '';'') SubscriberLanguages, dbo.fn_GreatestDate(MAX(s.CreateDate), MAX(slp.ModifyDate), default, default, default) ModifyDate
        FROM Subscriber s
        LEFT JOIN SubscriberLanguageProficiencies slp ON s.SubscriberId = slp.SubscriberId
        LEFT JOIN ProficiencyLevels pl ON slp.ProficiencyLevelId = pl.ProficiencyLevelId
		LEFT JOIN Languages l on slp.LanguageId = l.LanguageId
        WHERE s.IsDeleted = 0 and slp.IsDeleted = 0 and pl.IsDeleted = 0 and l.IsDeleted = 0
        GROUP BY s.SubscriberId			 
	 ), 
	 subscriberEmploymentType AS (
	
	 	SELECT s.SubscriberId, STRING_AGG(et.Name, '';'') EmploymentTypes
        FROM Subscriber s
        LEFT JOIN SubscriberEmploymentTypes e ON s.SubscriberId = e.SubscriberId
        LEFT JOIN dbo.EmploymentType et ON e.EmploymentTypeId = et.EmploymentTypeId
        WHERE s.IsDeleted = 0 and e.IsDeleted = 0 and et.IsDeleted = 0
        GROUP BY s.SubscriberId
			 
	 ),
	  subscriberTrainings AS (
	
		 SELECT s.SubscriberId, STRING_AGG( tt.Name + ''|'' + st.TrainingInstitution + ''|'' + st.TrainingName, '';'') SubscriberTraining
        FROM Subscriber s
        LEFT JOIN SubscriberTraining st ON s.SubscriberId = st.SubscriberId
        LEFT JOIN TrainingType tt ON st.TrainingTypeId = tt.TrainingTypeId 
        WHERE s.IsDeleted = 0 and st.IsDeleted = 0 and tt.IsDeleted = 0 
        GROUP BY s.SubscriberId	
			 
	 ),
	  subscriberEducation AS (
	 
 		SELECT s.SubscriberId, STRING_AGG(ISNULL(ei.Name,'''') + ''|'' +  ISNULL(edt.DegreeType,'''') + ''|'' + ISNULL(ed.Degree,'''')  , '';'') SubscriberEducation
        FROM Subscriber s
        LEFT JOIN  SubscriberEducationHistory seh ON s.SubscriberId = seh.SubscriberId
        LEFt JOIN EducationalInstitution ei ON seh.EducationalInstitutionId = ei.EducationalInstitutionId
		LEFT JOIN EducationalDegree ed on seh.EducationalDegreeId = ed.EducationalDegreeId
	    LEFT JOIN EducationalDegreeType edt ON seh.EducationalDegreeTypeId = edt.EducationalDegreeTypeId
        WHERE s.IsDeleted = 0 and seh.IsDeleted = 0 
        GROUP BY s.SubscriberId	
			 
	 ),
	 subscriberTitle AS (
	 
 		SELECT x.SubscriberId, STRING_AGG(ISNULL(x.Title,'''') , '';'') SubscriberTitles From
		(SELECT s.subscriberId,swh.Title
        FROM Subscriber s
        LEFT JOIN  SubscriberWorkHistory swh ON s.SubscriberId = swh.SubscriberId    
        WHERE s.IsDeleted = 0 and swh.IsDeleted = 0 and swh.Title IS NOT NULL
		UNION SELECT subscriberid,title FROM Subscriber WHERE title  IS NOT NULL) X
		GROUP BY x.SubscriberId
			 
	 )	 	 	
	 ,
	  subscriberWorkHistories AS (
	
 		SELECT s.SubscriberId, STRING_AGG(ISNULL(c.CompanyName,'''') + ''|'' +  ISNULL(swh.Title,'''')   , '';'') SubscriberWorkHistory
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
		   ,  CAST(s.CurrentRate as float ) CurrentRate
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
		AND hm.HiringManagerId IS NULL
      ')");

        }
    }
}
