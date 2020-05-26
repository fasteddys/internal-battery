USE [careercircledb]
GO

--ADD/UPDATE STATE data
;MERGE [careercircledb].[dbo].[State] AS t
USING (
		SELECT Distinct [Code] = newdata.ProvinceAbbr
			  ,[Name] = newdata.[ProvinceName]
			  ,[CountryId] = c.Countryid
			  ,[Sequence] = s.[Sequence]
			  ,[StateId] = s.[StateId]
			  ,[StateGuid] = s.[StateGuid]
			  ,[IsDeleted] = s.[IsDeleted]
			  ,[CreateDate] = s.[CreateDate]
			  ,[ModifyDate] = s.[ModifyDate]
			  ,[CreateGuid] = s.[CreateGuid]
			  ,[ModifyGuid] = s.[ModifyGuid]
		FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
		Left Join (
			Select s.* From [careercircledb]..[State] s
			Join [careercircledb]..[Country] c
			On c.CountryId = s.CountryId
			Where c.Code3 IN ('USA','CAN')
		) As s
		On s.code = newdata.provinceAbbr
		Join [careercircledb]..[Country] c
		On c.Code3 = newdata.CountryName
		--order by newdata.ProvinceAbbr

) AS s
ON s.[StateId] = t.[StateId] -- AND s.[CountryId] = t.[CountryId]
WHEN MATCHED THEN 
	Update Set
		t.[Code] = s.[Code]
	    ,t.[Name] = s.[Name]
	   --,t.[CountryId] = s.[CountryId]
	   ,t.[Sequence] = ISNULL(s.[Sequence], 999) --need a separate script to perform
	   ,t.[ModifyDate] = GETUTCDATE()
	   ,t.[ModifyGuid] = NewId()
WHEN NOT MATCHED THEN 
	INSERT
   (
	   [IsDeleted]
      ,[CreateDate]
      ,[ModifyDate]
      ,[CreateGuid]
      ,[ModifyGuid]
--      ,[StateId]
      ,[StateGuid]
      ,[Code]
      ,[Name]
      ,[CountryId]
      ,[Sequence]
   )
   VALUES
   (
	    0
      ,GETUTCDATE()
      , null --[ModifyDate]
      ,NewId()
      , null --[ModifyGuid]
    --  ,[StateId] = 
      ,NewId()
      ,s.Code
      ,s.Name
      ,s.[CountryId]
      ,ISNULL(s.[Sequence], 999)
   )
--WHEN NOT MATCHED BY SOURCE
--    THEN DELETE;
OUTPUT
   $action AS ActionType,
   GETUTCDATE() AS LogDateTime,
  -- deleted.*,
   inserted.*
INTO [DataImport].[StatePostalDataRefreshLog];


GO

--ADD/UPDATE CITY data


;MERGE [careercircledb].[dbo].[City] AS t
USING (

		SELECT DISTINCT [Name] = newdata.[CityName]
			  ,[StateId] = IsNull(olddata.[StateId], newdata.StateId)
			  ,[CityId] = olddata.[CityId]
			  ,[IsDeleted] = olddata.[IsDeleted]
			  ,[CreateDate] = olddata.[CreateDate]
			  ,[ModifyDate] = olddata.[ModifyDate]
			  ,[CreateGuid] = olddata.[CreateGuid]
			  ,[ModifyGuid] = olddata.[ModifyGuid]
			  ,[CityGuid] = olddata.[CityGuid]
		FROM 
		(
		  SELECT distinct 
			  [CityName]
			  ,[ProvinceName]
			  ,[ProvinceAbbr]
			  ,s.StateId
		  FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] new
		  join [careercircledb]..[State] s
			On s.Code = new.ProvinceAbbr and s.name = new.ProvinceName
			Join [careercircledb]..[Country] ct
			ON ct.CountryId = s.CountryId
			WHERE ct.Code3 IN ('USA','CAN')
		)AS newdata
		Left Join (
			SELECT c.[Name] AS CityName, c.Cityid, s.Code AS StateCode, s.StateId, s.[Name] AS StateName,
				   c.[IsDeleted], c.[CreateDate], c.[ModifyDate], c.[CreateGuid], c.[ModifyGuid], c.[CityGuid]
			FROM [careercircledb]..[City] c
			Right Join [careercircledb]..[State] s
			On s.StateId = c.StateId
			Join [careercircledb]..[Country] ct
			ON ct.CountryId = s.CountryId
			WHERE ct.Code3 IN ('USA','CAN')
			) as olddata
		ON olddata.StateCode = newdata.ProvinceAbbr AND olddata.CityName = newdata.CityName

) AS s
ON  s.[CityId] = t.[CityId] AND s.[StateId] = t.[StateId]
WHEN MATCHED THEN 
	Update Set
	    t.[Name] = s.[Name]
	   ,t.[ModifyDate] = convert(datetime2,GETUTCDATE())
	   ,t.[ModifyGuid] = NewId()
WHEN NOT MATCHED THEN 
	INSERT
   (
		--[CityId]
       [IsDeleted]
      ,[CreateDate]
      ,[ModifyDate]
      ,[CreateGuid]
      ,[ModifyGuid]
      ,[CityGuid]
      ,[Name]
      ,[StateId]
   )
   VALUES
   (
	    0
      ,convert(datetime2,GETUTCDATE())
      , null --[ModifyDate]
      ,NewId()
      , null --[ModifyGuid]
      ,NewId()
      ,s.[Name]
      ,s.[StateId]
   )
--WHEN NOT MATCHED BY SOURCE
--    THEN DELETE;
OUTPUT
   $action AS ActionType,
   convert(datetime2,GETUTCDATE()) AS LogDateTime,
  -- deleted.*,
   inserted.*
INTO [DataImport].[CityPostalDataRefreshLog];

GO

--ADD/UPDATE ZIPCODE data


;MERGE [careercircledb].[dbo].[Postal] AS t
USING (

	SELECT [PostalId] = p.PostalId
		  ,[IsDeleted] = p.IsDeleted
		  ,[CreateDate] = p.CreateDate
		  ,[ModifyDate] = p.ModifyDate
		  ,[CreateGuid] = p.CreateGuid
		  ,[ModifyGuid] = p.ModifyGuid
		  ,[PostalGuid] = p.PostalGuid
		  ,[Code] = newdata.PostalCode
		  ,[Latitude] = newdata.Latitude
		  ,[Longitude] = newdata.[Longitude]
		  ,[CityId] = newdata.CityId
			FROM  (
				Select c.Name, c.CityId, s.Code, ct.Code3 AS CountryAbbr3, new.PostalCode, new.Longitude,new.Latitude
				From [careercircledb]..[City] c
				Join [careercircledb]..[State] s
				On s.StateId = c.StateId 
				Join [careercircledb]..[Country] ct
				ON ct.CountryId = s.CountryId
				Join [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS new
				On s.Code = new.ProvinceAbbr AND c.Name = new.CityName
				WHERE ct.Code3 IN ('USA','CAN')
			) as newdata
			Left Join [careercircledb].[dbo].[Postal] p
			On p.code = newdata.postalCode AND p.CityId = newdata.CityId
) AS s
ON s.[Code] = t.[Code] AND s.CityId = t.CityId
WHEN MATCHED THEN 
	Update Set
	    t.[ModifyDate] = GETUTCDATE()
	   ,t.[ModifyGuid] = NewId()
	   ,t.[Code] = s.[Code]
	   ,t.[Latitude] = s.[Latitude]
	   ,t.[Longitude] = s.[Longitude]
	   --,t.[CityId] = s.CityId
WHEN NOT MATCHED THEN 
	INSERT
   (
   --[PostalId]
		   [IsDeleted] 
		  ,[CreateDate]
		  ,[ModifyDate]
		  ,[CreateGuid]
		  ,[ModifyGuid]
		  ,[PostalGuid]
		  ,[Code]
		  ,[Latitude]
		  ,[Longitude]
		  ,[CityId]
   )
   VALUES
   (
	    0
      ,GETUTCDATE()
      , null --[ModifyDate]
      ,NewId()
      , null --[ModifyGuid]
      ,NewId()
	  ,s.[Code]
      ,s.[Latitude]
      ,s.[Longitude]
	  ,s.[CityId]
   )
--WHEN NOT MATCHED BY SOURCE
--    THEN DELETE;
OUTPUT
   $action AS ActionType,
   convert(datetime2,GETUTCDATE()) AS LogDateTime,
  -- deleted.*,
	inserted.*
INTO [DataImport].[ZipCodePostalDataRefreshLog];