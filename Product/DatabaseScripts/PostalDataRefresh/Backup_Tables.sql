
/*
Notes: 
=>The below script takes backup of the existing City, State and Postal table. 
=>Please name the backup tables with a date extension for convenience, for example, Postal_Backup_YYYYMMDD.
=>The script creates new log tables if the log tables do not exist, else it would truncate the data in tables, if they existed.
=>The below query will create a backup of the table data only, all PK - FK relationships will drop along with constraints.
=>Please Select the correct Database instance before running this script.
*/
Select * Into [dbo].[Postal_Backup_20200519] from [dbo].[Postal] 

GO

Select * Into [dbo].[City_Backup_20200519] from [dbo].[City] 

GO

Select * Into [dbo].[State_Backup_20200519] from [dbo].[State] 

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