using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddsprocSystem_Create_Update_LandingPageTrackingEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UIX_SubscriberEmploymentTypes_Subscriber_EmploymentType",
                table: "SubscriberEmploymentTypes",
                newName: "UIX_Tracking_SourceSlug");

            migrationBuilder.Sql(@"EXEC('/*
			-- =======================================================================================================
			-- Author:		Vivek Dutta
			-- Create date: 07/27/2020
			-- Description:	This sproc looks for a full url if it finds one it would log the tracking event.            
			--				If the url is not found it would insert then log the tracking event.
			-- Example:
			-- EXEC [dbo].[System_Create_Update_LandingPageTrackingEvent] @url = ''https://microsoft.com''
			-- ==========================================================================================================
			*/
			CREATE PROCEDURE [dbo].[System_Create_Update_LandingPageTrackingEvent] (
			   @url varchar(2048)
			)
			AS
			BEGIN
				-- SET NOCOUNT ON added to prevent extra result sets from
				-- interfering with SELECT statements.
				SET NOCOUNT ON;
			declare @trackingId int

			BEGIN TRY  
				Select 	@trackingId = TrackingId, @url = [Url]
				From [dbo].Tracking
				Where [Url] = @Url

			IF (IsNull(@trackingId, 0) < = 0)
			BEGIN
				Print N''Track record not found.'';
				Insert Into [dbo].Tracking
				Select [IsDeleted] = 0
				  ,[CreateDate] = GETUTCDATE()
				  ,[ModifyDate] = null
				  ,[CreateGuid] = ''00000000-0000-0000-0000-000000000000''
				  ,[ModifyGuid] = null
				  ,[Url] = @url
				  ,[SourceSlug] = null
				  ,[TrackingGuid] = NewId()

				;Select @trackingId = TrackingId, @url = [Url]
				From [dbo].Tracking
				Where [Url] = @Url
			END

			;MERGE [dbo].[TrackingEventDay] AS t
			USING (
				Select
					   [IsDeleted]
					  ,[CreateDate]
					  ,[ModifyDate]
					  ,[CreateGuid]
					  ,[ModifyGuid]
					  ,[TrackingEventDayGuid]
					  ,[Day]
					  ,[Count]
					  ,[TrackingId]
				From [dbo].[TrackingEventDay]
				Where TrackingId = @trackingId
				And [Day] = Cast(GetUtcDate() AS Date)

				UNION ALL
				--Default record when above select does not return a [TrackingEventDay] record
				Select
					   [IsDeleted] = 0
					  ,[CreateDate] = GetUtcDate()
					  ,[ModifyDate] = null
					  ,[CreateGuid] = ''00000000-0000-0000-0000-000000000000''
					  ,[ModifyGuid] = null
					  ,[TrackingEventDayGuid] = NewId()
					  ,[Day] = Cast(GetUtcDate() AS Date)
					  ,[Count] = 1
					  ,[TrackingId] = @trackingId
				WHERE NOT EXISTS ( SELECT * From [dbo].[TrackingEventDay]
									WHERE TrackingId = @trackingId
									And [Day] = Cast(GetUtcDate() AS Date))
			) AS s
			ON s.[Day] = t.[Day] AND s.[TrackingId] = t.[TrackingId]
			WHEN MATCHED
				THEN Update Set
				t.[ModifyDate] = GetUtcDate(),
				t.[Count] = s.[Count] + 1
			WHEN NOT MATCHED BY TARGET
				THEN Insert 
				(
					   [IsDeleted]
					  ,[CreateDate]
					  ,[CreateGuid]
					  ,[TrackingEventDayGuid]
					  ,[Day]
					  ,[Count]
					  ,[TrackingId]
				)
				Values (
					   s.[IsDeleted]
					  ,s.[CreateDate]
					  ,s.[CreateGuid]
					  ,s.[TrackingEventDayGuid]
					  ,s.[Day]
					  ,s.[Count]
					  ,s.[TrackingId]
				);
			END TRY  
			BEGIN CATCH  
				SELECT  
					 ERROR_NUMBER() AS ErrorNumber  
					,ERROR_SEVERITY() AS ErrorSeverity  
					,ERROR_STATE() AS ErrorState  
					,ERROR_PROCEDURE() AS ErrorProcedure  
					,ERROR_LINE() AS ErrorLine  
					,ERROR_MESSAGE() AS ErrorMessage;  

			END CATCH;  

			END

			')");    
                
         }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UIX_Tracking_SourceSlug",
                table: "SubscriberEmploymentTypes",
                newName: "UIX_SubscriberEmploymentTypes_Subscriber_EmploymentType");

			migrationBuilder.Sql(@"EXEC('/*
			-- =======================================================================================================
			-- Author:		Vivek Dutta
			-- Create date: 07/27/2020
			-- Description:	This sproc looks for a full url if it finds one it would log the tracking event.            
			--				If the url is not found it would insert then log the tracking event.
			-- Example:
			-- EXEC [dbo].[System_Create_Update_LandingPageTrackingEvent] @url = ''https://microsoft.com''
			-- ==========================================================================================================
			*/
			CREATE PROCEDURE [dbo].[System_Create_Update_LandingPageTrackingEvent] (
			   @url varchar(2048)
			)
			AS
			BEGIN
				-- SET NOCOUNT ON added to prevent extra result sets from
				-- interfering with SELECT statements.
				SET NOCOUNT ON;
			declare @trackingId int

			BEGIN TRY  
				Select 	@trackingId = TrackingId, @url = [Url]
				From [dbo].Tracking
				Where [Url] = @Url

			IF (IsNull(@trackingId, 0) < = 0)
			BEGIN
				Print N''Track record not found.'';
				Insert Into [dbo].Tracking
				Select [IsDeleted] = 0
				  ,[CreateDate] = GETUTCDATE()
				  ,[ModifyDate] = null
				  ,[CreateGuid] = ''00000000-0000-0000-0000-000000000000''
				  ,[ModifyGuid] = null
				  ,[Url] = @url
				  ,[SourceSlug] = null
				  ,[TrackingGuid] = NewId()

				;Select @trackingId = TrackingId, @url = [Url]
				From [dbo].Tracking
				Where [Url] = @Url
			END

			;MERGE [dbo].[TrackingEventDay] AS t
			USING (
				Select
					   [IsDeleted]
					  ,[CreateDate]
					  ,[ModifyDate]
					  ,[CreateGuid]
					  ,[ModifyGuid]
					  ,[TrackingEventDayGuid]
					  ,[Day]
					  ,[Count]
					  ,[TrackingId]
				From [dbo].[TrackingEventDay]
				Where TrackingId = @trackingId
				And [Day] = Cast(GetUtcDate() AS Date)

				UNION ALL
				--Default record when above select does not return a [TrackingEventDay] record
				Select
					   [IsDeleted] = 0
					  ,[CreateDate] = GetUtcDate()
					  ,[ModifyDate] = null
					  ,[CreateGuid] = ''00000000-0000-0000-0000-000000000000''
					  ,[ModifyGuid] = null
					  ,[TrackingEventDayGuid] = NewId()
					  ,[Day] = Cast(GetUtcDate() AS Date)
					  ,[Count] = 1
					  ,[TrackingId] = @trackingId
				WHERE NOT EXISTS ( SELECT * From [dbo].[TrackingEventDay]
									WHERE TrackingId = @trackingId
									And [Day] = Cast(GetUtcDate() AS Date))
			) AS s
			ON s.[Day] = t.[Day] AND s.[TrackingId] = t.[TrackingId]
			WHEN MATCHED
				THEN Update Set
				t.[ModifyDate] = GetUtcDate(),
				t.[Count] = s.[Count] + 1
			WHEN NOT MATCHED BY TARGET
				THEN Insert 
				(
					   [IsDeleted]
					  ,[CreateDate]
					  ,[CreateGuid]
					  ,[TrackingEventDayGuid]
					  ,[Day]
					  ,[Count]
					  ,[TrackingId]
				)
				Values (
					   s.[IsDeleted]
					  ,s.[CreateDate]
					  ,s.[CreateGuid]
					  ,s.[TrackingEventDayGuid]
					  ,s.[Day]
					  ,s.[Count]
					  ,s.[TrackingId]
				);
			END TRY  
			BEGIN CATCH  
				SELECT  
					 ERROR_NUMBER() AS ErrorNumber  
					,ERROR_SEVERITY() AS ErrorSeverity  
					,ERROR_STATE() AS ErrorState  
					,ERROR_PROCEDURE() AS ErrorProcedure  
					,ERROR_LINE() AS ErrorLine  
					,ERROR_MESSAGE() AS ErrorMessage;  

			END CATCH;  

			END

			')");

		}
	}
}
