/*
ROLLBACK Postal data refresh
*/
USE [careercircledb] --Change based on environment
GO


--RollBack ZIPCODE data


;MERGE [dbo].[Postal] AS t
USING (

	SELECT [PostalId]
      ,[IsDeleted]
      ,[CreateDate]
      ,[ModifyDate]
      ,[CreateGuid]
      ,[ModifyGuid]
      ,[PostalGuid]
      ,[Code]
      ,[Latitude]
      ,[Longitude]
      ,[CityId]
	FROM [dbo].[Postal_BackUp_20200519]

) AS s
ON s.[PostalId] = t.[PostalId]
WHEN MATCHED THEN 
	Update Set
	    t.[ModifyDate] = GetDate()
	   ,t.[ModifyGuid] = NewId()
	   ,t.[Code] = s.[Code]
	   ,t.[Latitude] = s.[Latitude]
	   ,t.[Longitude] = s.[Longitude]
	   ,t.[CityId] = s.CityId
	   ,t.[IsDeleted] = s.[IsDeleted]
WHEN NOT MATCHED BY SOURCE
    THEN DELETE 
OUTPUT
   $action AS ActionType,
   deleted.*,
   inserted.*;

   GO
--Rollback CITY data


;MERGE [dbo].[City] AS t
USING (

		SELECT [CityId]
			  ,[IsDeleted]
			  ,[CreateDate]
			  ,[ModifyDate]
			  ,[CreateGuid]
			  ,[ModifyGuid]
			  ,[CityGuid]
			  ,[Name]
			  ,[StateId]
		 FROM [dbo].[City_BackUp_20200519]
) AS s
ON s.[CityId] = t.[CityId]
WHEN MATCHED THEN 
	Update Set
	    t.[Name] = s.[Name]
	   ,t.[ModifyDate] = GetDate()
	   ,t.[ModifyGuid] = NewId()
	   ,t.[IsDeleted] = s.[IsDeleted]
WHEN NOT MATCHED BY SOURCE
    THEN DELETE 
OUTPUT
   $action AS ActionType,
   deleted.*,
   inserted.*;

GO

--Rollback STATE data
;MERGE [dbo].[State] AS t
USING (
		SELECT Distinct [IsDeleted]
		  ,[CreateDate]
		  ,[ModifyDate]
		  ,[CreateGuid]
		  ,[ModifyGuid]
		  ,[StateId]
		  ,[StateGuid]
		  ,[Code]
		  ,[Name]
		  ,[CountryId]
		  ,[Sequence]
		FROM [dbo].[State_BackUp_20200519] 
) AS s
ON s.[StateId] = t.[StateId]
WHEN MATCHED THEN 
	Update Set
		t.[Code] = s.[Code]
	   ,t.[Name] = s.[Name]
	   ,t.[CountryId] = s.[CountryId]
	   ,t.[Sequence] = s.[Sequence]
	   ,t.[ModifyDate] = GetDate()
	   ,t.[ModifyGuid] = NewId()
	   ,t.[IsDeleted] = t.[IsDeleted]

WHEN NOT MATCHED BY SOURCE
    THEN DELETE
OUTPUT
   $action AS ActionType,
   deleted.*,
   inserted.*;


GO

