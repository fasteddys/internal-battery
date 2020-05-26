USE [careercircledb]
GO
/*
Note: The below query will create a backup of the table data only, all PK - FK relationships will drop along with constraints.
*/
Select * Into [Postal_BackUp_20200519] from [dbo].[Postal] 

GO

Select * Into [City_BackUp_20200519] from [dbo].[City] 

GO

Select * Into [State_BackUp_20200519] from [dbo].[State] 

GO

--Select * Into [Country_BackUp_20200519] from [Country] 

--GO

/*
Create log tables
*/

--City

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
			WHERE TABLE_SCHEMA = 'DataImport' 
			AND TABLE_NAME = 'CityPostalDataRefreshLog')
BEGIN
	TRUNCATE TABLE [DataImport].CityPostalDataRefreshLog;
END

ELSE 
BEGIN 
	Create Table [DataImport].CityPostalDataRefreshLog
	(
		 [LogId] int NOT NULL PRIMARY KEY IDENTITY(1,1)
		,[ActionType] varchar(15)
		,[LogDateTime] datetime2(7)
		,[CityId] int
		,[IsDeleted] int
		,[CreateDate] datetime2(7)
		,[ModifyDate] datetime2(7)
		,[CreateGuid] uniqueidentifier
		,[ModifyGuid] uniqueidentifier
		,[CityGuid] uniqueidentifier
		,[Name] varchar(100)
		,[StateId] int
	)
END

GO

--State

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
			WHERE TABLE_SCHEMA = 'DataImport' 
			AND TABLE_NAME = 'StatePostalDataRefreshLog')
BEGIN
	TRUNCATE TABLE [DataImport].StatePostalDataRefreshLog
END

ELSE 
BEGIN 
	Create Table [DataImport].StatePostalDataRefreshLog
	(
		   [LogId] int NOT NULL PRIMARY KEY IDENTITY(1,1)
		  ,[ActionType] varchar(15)
		  ,[LogDateTime] datetime2(7)
		  ,[IsDeleted] int
		  ,[CreateDate] datetime2(7)
		  ,[ModifyDate] datetime2(7)
		  ,[CreateGuid] uniqueidentifier
		  ,[ModifyGuid] uniqueidentifier
		  ,[StateId] int
		  ,[StateGuid] uniqueidentifier
		  ,[Code] varchar(5)
		  ,[Name] varchar(50)
		  ,[CountryId] int
		  ,[Sequence] int
	)
END

GO

--Postal

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
			WHERE TABLE_SCHEMA = 'DataImport' 
			AND TABLE_NAME = 'ZipCodePostalDataRefreshLog')
BEGIN
	TRUNCATE TABLE [DataImport].ZipCodePostalDataRefreshLog
END

ELSE 
BEGIN 
	Create Table [DataImport].ZipCodePostalDataRefreshLog
	(
		 [LogId] int NOT NULL PRIMARY KEY IDENTITY(1,1)
		,[ActionType] varchar(15)
		,[LogDateTime] datetime2(7)
		,[PostalId] int
		,[IsDeleted] int
		,[CreateDate] datetime2(7)
		,[ModifyDate] datetime2(7)
		,[CreateGuid] uniqueidentifier
		,[ModifyGuid] uniqueidentifier
		,[PostalGuid] uniqueidentifier
		,[Code] nvarchar(10)
		,[Latitude] decimal(12,9)
		,[Longitude] decimal(12,9)
		,[CityId] int
	)
END


GO