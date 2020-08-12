using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class AddsprocSystem_Get_FullUrlFromSlugTrackingEventLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
			-- =============================================
			-- Author:		Vivek Dutta
			-- Create date: 07/27/2020
			-- Description:	This sproc gets the full url from 
			--              a page slug name and logs a track
			--              event of the call.
			-- Example:
			-- EXEC [dbo].[System_Get_FullUrlFromSlugTrackingEventLog] @sourceSlug = ''microsoft''
			-- =============================================
			*/
			CREATE PROCEDURE [dbo].[System_Get_FullUrlFromSlugTrackingEventLog] 
			   @sourceSlug varchar(50),
			   @url varchar(2048) OUTPUT
			AS
			BEGIN
				-- SET NOCOUNT ON added to prevent extra result sets from
				-- interfering with SELECT statements.
				SET NOCOUNT ON;
			declare @trackingId int

			BEGIN TRY  

			;WITH trackRecord(TrackingId, [Url], SourceSlug)
			AS (
				Select TrackingId, [Url], SourceSlug
				From [dbo].Tracking
				Where SourceSlug = @sourceSlug
			)

			Select 	@trackingId = TrackingId, @url = [Url] from trackRecord;
			--Select 	@trackingId, @url
			IF (IsNull(@trackingId, 0) < = 0)
			BEGIN
				Print N''Track record not found.''
				return @url
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

				return @url
			END CATCH;  

			END
			')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"EXEC('
				DROP PROCEDURE [dbo].[System_Get_FullUrlFromSlugTrackingEventLog]
			')");
		}
	}
}
