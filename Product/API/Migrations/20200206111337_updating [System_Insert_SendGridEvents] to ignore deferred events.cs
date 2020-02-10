using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class updatingSystem_Insert_SendGridEventstoignoredeferredevents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC(' 
/*
<remarks>
2020-01-16 - Jim Brazil - Created
2020-02-06 - Jim Brazil - Modified to not track deferred email events
</remarks>
<description>
Handles bulk inserting sengrid events
</description>
<example>
 
EXEC [dbo].[[System_Insert_SendGridEvents]] @SenGridJson=eventJson

</example>
*/
ALTER PROCEDURE [dbo].[System_Insert_SendGridEvents] (
	@SendGridJson VarChar(Max)
)
AS
BEGIN
 
	BEGIN TRANSACTION;
	BEGIN TRY 	
 	
	  SELECT
	   0 as IsDeleted,
	   GETDATE() as CreateDate,
	   CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as CreateGuid,
       GETDATE() as ModifyDate,
        CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as ModifyGuid,
       NewId() as SendGridEventGuid,
       json.*	  	  
	   INTO #Events
		FROM OPENJSON(
			@SendGridJson
		)
		WITH 
		(
			Email varchar(MAX),
			Timestamp BigInt,
			smtp_id varchar(MAX),
			Event varchar(MAX),
			Category varchar(MAX),
			Sg_event_id varchar(MAX),
			Sg_message_id varchar(MAX),
			Response varchar(MAX),
			attempt varchar(MAX),
			Ip varchar(Max),
			UserAgent varchar(MAX),
			Reason varchar(MAX),
			Status varchar(MAX),
			Tls varchar(MAX),
			url varchar(MAX),
			Type varchar(MAX),
			Marketing_campaign_id varchar(MAX),
			Marketing_campaign_name varchar(MAX),
			Subject varchar(MAX)
		) as json

	 /* Insert into the SendGridEvent audit table */
      INSERT INTO SendGridEvent 
	  (
	     IsDeleted,
		 CreateDate,
		 CreateGuid,
		 ModifyDate,
		 ModifyGuid,
		 SendGridEventGuid,
		 Email,
		 [Timestamp],
		 smtp_id,
		 [Event],
		 Category,
		 Sg_event_id,
		 Sg_message_id,
		 Response,attempt,
		 [Ip],
		 UserAgent,
		 Reason,
		 [Status],
		 Tls,
		 [url],
		 [Type],
		 Marketing_campaign_id,
		 Marketing_campaign_name,
		 [Subject]
	  ) 
	  SELECT	
		 IsDeleted,
		 CreateDate,
		 CreateGuid,
		 ModifyDate,
		 ModifyGuid,
		 SendGridEventGuid,
		 Email,
		 [Timestamp],
		 smtp_id,
		 [Event],
		 Category,
		 Sg_event_id,
		 Sg_message_id,
		 Response,
		 attempt,
		 [Ip],
		 UserAgent,
		 Reason,
		 [Status],
		 Tls,
		 [url],
		 [Type],
		 Marketing_campaign_id,
		 Marketing_campaign_name,
		 [Subject] 
	 FROM 
	    #Events 
 
	 INSERT INTO 
		SubscriberSendGridEvent 
		(
			IsDeleted,
			CreateDate,
			ModifyDate,
			CreateGuid,
			ModifyGuid,
			SubscriberSendGridEventGuid,
			SubscriberId,
			[Event],
			Category,
			EventStatus,
			[Type],
			Marketing_campaign_id,
			Marketing_campaign_name,
			[Subject],
			Sg_message_id,
			Response,
			Reason,
			[Status],
			Attempt,
			email
	   ) 
	  SELECT 	
			0,
			GETUTCDATE(),
			GETUTCDATE(),
		    CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as CreateGuid,       
			CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as ModifyGuid,
			NEWID(),
			s.SubscriberId,			
			e.[Event],
			e.Category,
			0,
			e.[Type],		
			e.Marketing_campaign_id,
			e.Marketing_campaign_name,
			e.[Subject], 		
			e.Sg_message_id,
			e.Response,
			e.Reason,			
			e.[Status],
			e.Attempt,
			e.email	
		FROM 
		   #Events e
		   left join Subscriber s on e.Email = s.Email
		   where e.[Event] <> ''deferred''  
		  
	END TRY
	BEGIN CATCH
		SELECT 
			 ERROR_NUMBER() AS ErrorNumber
			,ERROR_SEVERITY() AS ErrorSeverity
			,ERROR_STATE() AS ErrorState
			,ERROR_PROCEDURE() AS ErrorProcedure
			,ERROR_LINE() AS ErrorLine
			,ERROR_MESSAGE() AS ErrorMessage;
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
	END CATCH;

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION;
 
END
            ')");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"EXEC(' 
/*
<remarks>
2020-01-16 - Jim Brazil - Created
</remarks>
<description>
Handles bulk inserting sengrid events
</description>
<example>
 
EXEC [dbo].[[System_Insert_SendGridEvents]] @SenGridJson=eventJson

</example>
*/
CREATE PROCEDURE [dbo].[System_Insert_SendGridEvents] (
	@SendGridJson VarChar(Max)
)
AS
BEGIN
 
	BEGIN TRANSACTION;
	BEGIN TRY 	
 	
	  SELECT
	   0 as IsDeleted,
	   GETDATE() as CreateDate,
	   CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as CreateGuid,
       GETDATE() as ModifyDate,
        CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as ModifyGuid,
       NewId() as SendGridEventGuid,
       json.*	  	  
	   INTO #Events
		FROM OPENJSON(
			@SendGridJson
		)
		WITH 
		(
			Email varchar(MAX),
			Timestamp BigInt,
			smtp_id varchar(MAX),
			Event varchar(MAX),
			Category varchar(MAX),
			Sg_event_id varchar(MAX),
			Sg_message_id varchar(MAX),
			Response varchar(MAX),
			attempt varchar(MAX),
			Ip varchar(Max),
			UserAgent varchar(MAX),
			Reason varchar(MAX),
			Status varchar(MAX),
			Tls varchar(MAX),
			url varchar(MAX),
			Type varchar(MAX),
			Marketing_campaign_id varchar(MAX),
			Marketing_campaign_name varchar(MAX),
			Subject varchar(MAX)
		) as json

	 /* Insert into the SendGridEvent audit table */
      INSERT INTO SendGridEvent 
	  (
	     IsDeleted,
		 CreateDate,
		 CreateGuid,
		 ModifyDate,
		 ModifyGuid,
		 SendGridEventGuid,
		 Email,
		 [Timestamp],
		 smtp_id,
		 [Event],
		 Category,
		 Sg_event_id,
		 Sg_message_id,
		 Response,attempt,
		 [Ip],
		 UserAgent,
		 Reason,
		 [Status],
		 Tls,
		 [url],
		 [Type],
		 Marketing_campaign_id,
		 Marketing_campaign_name,
		 [Subject]
	  ) 
	  SELECT	
		 IsDeleted,
		 CreateDate,
		 CreateGuid,
		 ModifyDate,
		 ModifyGuid,
		 SendGridEventGuid,
		 Email,
		 [Timestamp],
		 smtp_id,
		 [Event],
		 Category,
		 Sg_event_id,
		 Sg_message_id,
		 Response,
		 attempt,
		 [Ip],
		 UserAgent,
		 Reason,
		 [Status],
		 Tls,
		 [url],
		 [Type],
		 Marketing_campaign_id,
		 Marketing_campaign_name,
		 [Subject] 
	 FROM 
	    #Events 
 
	 INSERT INTO 
		SubscriberSendGridEvent 
		(
			IsDeleted,
			CreateDate,
			ModifyDate,
			CreateGuid,
			ModifyGuid,
			SubscriberSendGridEventGuid,
			SubscriberId,
			[Event],
			Category,
			EventStatus,
			[Type],
			Marketing_campaign_id,
			Marketing_campaign_name,
			[Subject],
			Sg_message_id,
			Response,
			Reason,
			[Status],
			Attempt,
			email
	   ) 
	  SELECT 	
			0,
			GETUTCDATE(),
			GETUTCDATE(),
		    CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as CreateGuid,       
			CAST(''00000000-0000-0000-0000-000000000000'' as UNIQUEIDENTIFIER) as ModifyGuid,
			NEWID(),
			s.SubscriberId,			
			e.[Event],
			e.Category,
			0,
			e.[Type],		
			e.Marketing_campaign_id,
			e.Marketing_campaign_name,
			e.[Subject], 		
			e.Sg_message_id,
			e.Response,
			e.Reason,			
			e.[Status],
			e.Attempt,
			e.email	
		FROM 
		   #Events e
		   left join Subscriber s on e.Email = s.Email
		  
	END TRY
	BEGIN CATCH
		SELECT 
			 ERROR_NUMBER() AS ErrorNumber
			,ERROR_SEVERITY() AS ErrorSeverity
			,ERROR_STATE() AS ErrorState
			,ERROR_PROCEDURE() AS ErrorProcedure
			,ERROR_LINE() AS ErrorLine
			,ERROR_MESSAGE() AS ErrorMessage;
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
	END CATCH;

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION;
 
END
            ')");


        }
    }
}
