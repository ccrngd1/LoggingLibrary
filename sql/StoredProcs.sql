USE [ClaimsAuditWarehouse]
GO

/****** Object:  StoredProcedure [Logging].[GetLogsForTimeSpan]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Logging].[GetLogsForTimeSpan]

/***********************************************************************************************************
*   Procedure name: logging.GetLogsForTimeSpan
*   Description:   
*   Table(s):       (R) logging.AppLog
					(R) logging.LogServiceInstance
					(R) logging.LogServices
					(R) logging.LogMessages
					(R) logging.StackTraceLookup
					(R) logging.MessageLookup
*   Developer:      Nick Lawson
*   Creation Date:  07-27-2016
*
*   Change History:					
* 
************************************************************************************************************/
	@startDT DATETIME,
    @endDT DATETIME,
	@logLvl INT

AS
	BEGIN    
		
SELECT 
--		appLog.UUID ,
--       appLog.ServiceInstanceHash ,
       appLog.SeverityLevelId ,
       appLog.CallingMethodName ,
       appLog.InFlightPayload ,
       appLog.LogTimeStamp ,
--       Inst.LogServiceInstanceHash ,
       Inst.UniversalAppPath ,
--       Inst.LogServiceHash ,
       Inst.AppVersion ,
       Inst.BuildDate ,
--       Serv.ServiceHash ,
       Serv.AppName,
	   msgLookup.MessageText,
	   stackTrace.StackText
	   
FROM ClaimsAuditWarehouse.Logging.AppLog appLog
INNER JOIN ClaimsAuditWarehouse.Logging.LogServiceInstances Inst ON inst.LogServiceInstanceHash= appLog.ServiceInstanceHash
INNER JOIN ClaimsAuditWarehouse.Logging.LogServices Serv ON serv.ServiceHash= inst.LogServiceHash
INNER JOIN ClaimsAuditWarehouse.Logging.LogMessages msgs ON msgs.LogUUID = appLog.UUID
LEFT JOIN ClaimsAuditWarehouse.Logging.StackTraceLookup stackTrace ON stackTrace.Hash = msgs.StackTraceHash
LEFT JOIN ClaimsAuditWarehouse.Logging.MessageLookup msgLookup ON msgLookup.Hash = msgs.LogMessageLookupHash
WHERE LogTimeStamp > @startDT AND LogTimeStamp < @endDT
AND appLog.SeverityLevelId >= @logLvl

	END;

GO

/****** Object:  StoredProcedure [Logging].[spA_CheckLogService]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create PROCEDURE [Logging].[spA_CheckLogService]
/***********************************************************************************************************
*   Procedure name: [spA_CheckLogService]
*   Description:    This SP ensure the log service exists in the db
*					(R) LogService 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@serviceNameHash VARCHAR(64),
@serviceName VARCHAR(100)
AS
BEGIN 

MERGE logging.LogServices AS targetLogSvc
USING (VALUES(@serviceNameHash, @serviceName) )AS srcLogSvc(id,name)
ON targetLogSvc.ServiceHash = srcLogSvc.id
WHEN NOT MATCHED THEN INSERT 
	VALUES(srcLogSvc.id, srcLogSvc.name);

END

GO

/****** Object:  StoredProcedure [Logging].[spA_CheckLogServiceInstance]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Logging].[spA_CheckLogServiceInstance]
/***********************************************************************************************************
*   Procedure name: [spA_CheckLogServiceInstance]
*   Description:    This SP ensure the log service instance exists in the db
*					(R) LogServiceInstances 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@serviceNameInstanceHash VARCHAR(64),
@serviceHash VARCHAR(64),
@serviceInstanceUniversalPath VARCHAR(MAX),
@serviceInstanceVersion VARCHAR(15),
@serviceInstanceBuildDate DATETIME = null

AS
BEGIN 

MERGE logging.LogServiceInstances AS targetLogSvcInst
USING (VALUES(@serviceNameInstanceHash,@serviceHash, @serviceInstanceUniversalPath, @serviceInstanceVersion, null) )AS srcLogSvc(instHash, svcHash, svcPath, svcVersion, svcBuild)
ON targetLogSvcInst.LogServiceInstanceHash = srcLogSvc.instHash
WHEN NOT MATCHED THEN INSERT 
	VALUES(srcLogSvc.instHash, srcLogSvc.svcPath, srcLogSvc.svcHash, srcLogSvc.svcVersion);

END

GO

/****** Object:  StoredProcedure [Logging].[spa_CullDaysOfLogging]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Logging].[spa_CullDaysOfLogging]
/***********************************************************************************************************
*   Procedure name: Logging.spa_CullDaysOfLogging
*   Description:    pass a number of days, anything past that will be culled
*   Table(s):       
*   Developer:      Nick Lawson
*   Creation Date:  10/12/2016
*
*   Change History:
*
************************************************************************************************************/
    @DaysToKeep int
AS
    BEGIN
	
DECLARE @appLogToCull TABLE (
uuid UNIQUEIDENTIFIER
)

INSERT INTO @appLogToCull
        ( uuid )
SELECT UUID FROM ClaimsAuditWarehouse.Logging.AppLog WHERE LogTimeStamp < DATEADD(DAY, -@DaysToKeep, GETDATE())

--SELECT * FROM @appLogToCull

DELETE from ClaimsAuditWarehouse.Logging.AppLog WHERE uuid IN (SELECT uuid FROM @appLogToCull)

--SELECT * FROM ClaimsAuditWarehouse.logging.CallingMethodIO WHERE LogUUID IN (SELECT uuid FROM @appLogToCull)

DELETE FROM ClaimsAuditWarehouse.Logging.CallingMethodIO WHERE  LogUUID IN (SELECT uuid FROM @appLogToCull)

--SELECT * FROM ClaimsAuditWarehouse.Logging.AdditionalData WHERE  LogUUID IN (SELECT uuid FROM @appLogToCull)

DELETE FROM ClaimsAuditWarehouse.Logging.AdditionalData WHERE  LogUUID IN (SELECT uuid FROM @appLogToCull)

--SELECT * FROM ClaimsAuditWarehouse.Logging.LogMessages WHERE LogUUID IN (SELECT uuid FROM @appLogToCull)

DELETE FROM ClaimsAuditWarehouse.Logging.LogMessages WHERE LogUUID IN (SELECT uuid FROM @appLogToCull)

END;
GO

/****** Object:  StoredProcedure [Logging].[spa_FilterLogDetails]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [Logging].[spa_FilterLogDetails]
@start datetime,
@end datetime,
@metaField varchar(255) = null,
@metaValue varchar(255)=null,
@messageText varchar(255)=null
as
begin
if(@start is null)
	set @start= DATEADD(d,-1,CURRENT_TIMESTAMP);
if(@end is null)
	set @end= CURRENT_TIMESTAMP;
if(@start>=@end)
	set @end = DATEADD(s,1,@start);

declare @lst dbo.ListGuid;
--print 'Start:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))

if(@metaField is not null and @metaValue is not null and @messageText is not null)
Begin
insert into @lst
	select al.UUID
	from logging.AdditionalData ad
		inner join logging.AppLog al on ad.LogUUID=al.UUID
		inner join logging.LogMessages(nolock)lm on al.UUID = lm.LogUUID	
		inner join logging.MessageLookup(nolock)ml on lm.LogMessageLookupHash=ml.Hash
	where 
		(al.LogTimeStamp between @start and @end) and
		((ad.KeyName like @metaField ) and
		(ad.Value =@metaValue) )
		and (ml.MessageText like @messageText)
		--print 'Insert Message and Meta:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
end
else
begin
if(@messageText is not null)
begin
	insert into @lst
		select al.UUID
		from
			logging.AppLog (nolock) al inner join
			logging.LogMessages(nolock)lm on al.UUID=lm.LogUUID inner join
			logging.MessageLookup(nolock) ml on  lm.LogMessageLookupHash =ml.Hash 
		where (al.LogTimeStamp between @start and @end) and
			 ml.MessageText like @messageText

	--print 'Message Text:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
end
if(@metaField is not null and @metaValue is not null)
begin


insert into @lst
	select al.UUID
	from logging.AdditionalData ad
		inner join logging.AppLog al on ad.LogUUID=al.UUID	
	where 
		(al.LogTimeStamp between @start and @end) and
		((ad.KeyName like @metaField ) and
		(ad.Value =@metaValue) )
	--	print 'Insert Meta:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))

end
end


select
	s.val as [Id]	,	
	ls.AppName,lsi.AppVersion,
	al.LogTimeStamp, al.CallingMethodName,al.InFlightPayload
	
from  
	@lst s 
	inner join  logging.AppLog (nolock) al on s.Val = al.UUID
	inner join logging.LogServiceInstances(nolock) lsi on al.ServiceInstanceHash=lsi.LogServiceInstanceHash
	inner join logging.LogServices(nolock) ls on lsi.LogServiceHash=ls.ServiceHash
	

--print 'Applog:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
select
	s.Val,
	lm.Depth, ml.MessageText,sl.StackText
from logging.MessageLookup(nolock) ml
	left outer join logging.LogMessages(nolock)lm on ml.Hash = lm.LogMessageLookupHash
	left outer join logging.StackTraceLookup(nolock)sl on  lm.StackTraceHash=sl.Hash
	inner join @lst s on lm.LogUUID = s.Val

--print 'message/stack:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
Select 
	s.Val as [Id],
	IsInput,ParameterName,Value
from Logging.CallingMethodIO inner join @lst s on Logging.CallingMethodIO.LogUUID=s.Val
--print 'CallingMethodIO:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
Select 
	s.Val as [Id],
	KeyName,Value
from [Logging].[AdditionalData] inner join @lst s on Logging.[AdditionalData].LogUUID=s.Val
--print 'AdditionalData:' +  convert(varchar(32), CONVERT(varchar, SYSDATETIME(), 121))
end


GO

/****** Object:  StoredProcedure [Logging].[spA_GeLogServiceInstancesById]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [Logging].[spA_GeLogServiceInstancesById]

/***********************************************************************************************************
*   Procedure name: logging.[spA_GetLatestLogServiceInstances]
*   Description:   
*   Table(s):       (R) logging.LogServices
					(R) logging.LogServiceInstances
*   Developer:      Josh.bartlett
*   Creation Date:  11-14-2016
*
*   Change History:					
* 
************************************************************************************************************/
	@ServiceInstanceHash varchar(64)
AS
	BEGIN    
	Select 
	ls.ServiceHash,
	AppName,
	LogServiceInstanceHash,
	UniversalAppPath,
	AppVersion
	From ClaimsAuditWarehouse.logging.LogServices ls
	 inner join ClaimsAuditWarehouse.logging.LogServiceInstances lsi on ls.ServiceHash = lsi.LogServiceHash where LogServiceInstanceHash=@ServiceInstanceHash
	END;

GO

/****** Object:  StoredProcedure [Logging].[spA_GeLogServiceInstancesByServiceId]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [Logging].[spA_GeLogServiceInstancesByServiceId]

/***********************************************************************************************************
*   Procedure name: logging.[spA_GeLogServiceInstancesByServiceId]
*   Description:   
*   Table(s):       (R) logging.LogServices
					(R) logging.LogServiceInstances
*   Developer:      Josh.bartlett
*   Creation Date:  11-14-2016
*
*   Change History:					
* 
************************************************************************************************************/
	@ServiceId varchar(64)
AS
BEGIN
	Select ls.ServiceHash,AppName,LogServiceInstanceHash,UniversalAppPath,AppVersion
	 From ClaimsAuditWarehouse.logging.LogServices ls inner join ClaimsAuditWarehouse.logging.LogServiceInstances lsi on ls.ServiceHash = lsi.LogServiceHash 
	 where ls.ServiceHash=@ServiceId
END;

GO

/****** Object:  StoredProcedure [Logging].[spa_GetDetailsById]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [Logging].[spa_GetDetailsById]
@errorId uniqueidentifier
as 
begin
select 
	logging.LogServices.AppName,
	logging.LogServiceInstances.AppVersion,
	Logging.LogServiceInstances.UniversalAppPath,
	logging.AppLog.LogTimeStamp, 
	logging.AppLog.SeverityLevelId,
	logging.AppLog.CallingMethodName, 
	logging.AppLog.InFlightPayload
 from
  Logging.AppLog 
  inner join logging.LogServiceInstances on logging.AppLog.ServiceInstanceHash= logging.LogServiceInstances.LogServiceInstanceHash
  inner join logging.LogServices on logging.LogServiceInstances.LogServiceHash= logging.LogServices.ServiceHash
  where UUID = @errorId;

select logging.LogMessages.Depth,logging.MessageLookup.MessageText, logging.StackTraceLookup.StackText
 from Logging.LogMessages 
 left outer join logging.MessageLookup on logging.LogMessages.LogMessageLookupHash= logging.MessageLookup.Hash
 left outer join logging.StackTraceLookup on logging.LogMessages.StackTraceHash = logging.StackTraceLookup.Hash
 where logging.LogMessages.LogUUID=@errorId 
 order by logging.logmessages.depth;

select isInput,parametername,value from logging.CallingMethodIO where logging.CallingMethodIO.LogUUID=@errorId;

select keyname,value from logging.AdditionalData where Logging.AdditionalData.LogUUID=@errorid;
end
GO

/****** Object:  StoredProcedure [Logging].[spA_GetLatestLogServiceInstances]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Logging].[spA_GetLatestLogServiceInstances]

/***********************************************************************************************************
*   Procedure name: logging.[spA_GetLatestLogServiceInstances]
*   Description:   
*   Table(s):       (R) logging.LogServices
					(R) logging.LogServiceInstances
*   Developer:      Josh.bartlett
*   Creation Date:  11-14-2016
*
*   Change History:					
* 
************************************************************************************************************/
	@ServiceHash varchar(64)=null
AS
	BEGIN    
	select 
		ls.ServiceHash,
		ls.AppName,
		lsi.LogServiceInstanceHash,
		lsi.UniversalAppPath,
		lsi.AppVersion
	from 
		logging.LogServices ls 
		inner join logging.LogServiceInstances lsi on ls.ServiceHash = lsi.LogServiceHash
		inner join 
			(			
				select logging.LogServiceInstances.LogServiceHash, 			
				max(convert(float,SUBSTRING(appversion,1+charindex('.',appversion,1+ CHARINDEX('.',appversion,0)),255) ) )as buildVersion	
				from logging.LogServiceInstances 
				group by logging.LogServiceInstances.LogServiceHash
			) as latest 
			on convert(float,SUBSTRING(appversion,1+charindex('.',appversion,1+ CHARINDEX('.',appversion,0)),255) )  = latest.buildVersion and lsi.LogServiceHash = latest.logservicehash
	where ls.ServiceHash =@ServiceHash or @ServiceHash is null;
	END;
GO

/****** Object:  StoredProcedure [Logging].[spA_GetLogServiceInstances]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [Logging].[spA_GetLogServiceInstances]

/***********************************************************************************************************
*   Procedure name: logging.GetLogsForTimeSpan
*   Description:   
*   Table(s):       (R) logging.LogServices
					(R) logging.LogServiceInstances
*   Developer:      Josh.bartlett
*   Creation Date:  11-14-2016
*
*   Change History:					
* 
************************************************************************************************************/
	
AS
	BEGIN    
		Select ls.ServiceHash,
			AppName,
			LogServiceInstanceHash,
			UniversalAppPath,
			AppVersion		 
		From ClaimsAuditWarehouse.logging.LogServices ls 
		inner join ClaimsAuditWarehouse.logging.LogServiceInstances lsi on ls.ServiceHash = lsi.LogServiceHash 
	END;

GO

/****** Object:  StoredProcedure [Logging].[spA_GetLogServices]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [Logging].[spA_GetLogServices]

/***********************************************************************************************************
*   Procedure name: logging.GetLogsForTimeSpan
*   Description:   
*   Table(s):       (R) logging.LogServices
					
*   Developer:      Josh.bartlett
*   Creation Date:  11-14-2016
*
*   Change History:					
* 
************************************************************************************************************/
	
AS
	BEGIN    
	
	Select ServiceHash,AppName From ClaimsAuditWarehouse.logging.LogServices
	END;

GO

/****** Object:  StoredProcedure [Logging].[spA_GetLogsForServiceId]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [Logging].[spA_GetLogsForServiceId]

/***********************************************************************************************************
*   Procedure name: logging.GetLogsForTimeSpan
*   Description:   
*   Table(s):       (R) logging.AppLog
					(R) logging.LogServiceInstance
					(R) logging.LogServices
					(R) logging.LogMessages
					(R) logging.StackTraceLookup
					(R) logging.MessageLookup
*   Developer:      Nick Lawson
*   Creation Date:  07-27-2016
*
*   Change History:					
* 
************************************************************************************************************/
	@serviceId varchar(64),
	@startDT DATETIME,
    @endDT DATETIME,
	@logLvl INT

AS
	BEGIN    
		
		select al.UUID, al.LogTimeStamp,al.CallingMethodName,al.InFlightPayload,
	lm.Depth,ml.MessageText,stl.StackText
from logging.AppLog al
	inner join logging.LogServiceInstances lsi on al.ServiceInstanceHash = lsi.LogServiceInstanceHash
	inner join logging.LogMessages lm on al.UUID = lm.LogUUID
	left outer join logging.MessageLookup ml on lm.LogMessageLookupHash=ml.Hash
	left outer join logging.StackTraceLookup stl on lm.StackTraceHash = stl.Hash
where  al.SeverityLevelId>=@logLvl and
	(al.LogTimeStamp between @startDT and @endDT) and
	lsi.LogServiceHash = @serviceId
order by LogTimeStamp,uuid,Depth

	END;

GO

/****** Object:  StoredProcedure [Logging].[spA_GetMessageSummary]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure [Logging].[spA_GetMessageSummary]
@sevLevel int =null,
@start datetime =null,
@end datetime =null
AS
Begin
	if(@sevLevel is null)
		set @sevLevel=2;
	if(@start is null)	
		set @start = CONVERT(Datetime, FLOOR(CONVERT(float,CURRENT_TIMESTAMP)))  
	if(@end is null)
		set @end =  CURRENT_TIMESTAMP;
		print convert(varchar(255),@sevLevel)
		print convert(varchar(255),@start)
		print convert(varchar(255),@end)
	select 	
		ls.AppName,	
		ml.MessageText,
		count(ml.MessageText)	
	from
		 logging.applog(nolock) ap 
		 inner join logging.LogServiceInstances as lsi on ap.ServiceInstanceHash=lsi.LogServiceInstanceHash 
		 inner join logging.LogServices ls on lsi.LogServiceHash=ls.ServiceHash
		 inner join logging.LogMessages lm on ap.UUID = lm.LogUUID
		 inner join logging.MessageLookup ml on lm.LogMessageLookupHash = ml.Hash
	 where ap.SeverityLevelId>=@sevLevel and ( ap.LogTimeStamp between @start and @end)
		and lm.Depth<2
	group by ls.AppName,ml.MessageText
	order by ls.AppName;
end
GO

/****** Object:  StoredProcedure [Logging].[spa_SaveAdditionalDatas]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [Logging].[spa_SaveAdditionalDatas]
/***********************************************************************************************************
*   Procedure name: [spa_SaveAdditionalDatas]
*   Description:    save off additional data
*					(R) AdditionalData 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@addlData logging.AdditionalDatas READONLY

AS
BEGIN 

INSERT INTO logging.AdditionalData
        ( LogUUID, KeyName, Value )  
		SELECT logUUID ,
               KeyName ,
               Value
		FROM @addlData 
END


GO

/****** Object:  StoredProcedure [Logging].[spa_saveLogMsg]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create PROCEDURE [Logging].[spa_saveLogMsg]
/***********************************************************************************************************
*   Procedure name: [spa_saveLogMsg]
*   Description:    save off log messages
*					(R) LogMessages 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@logMsgs logging.LogMessages READONLY

AS
BEGIN 

INSERT INTO logging.LogMessages
        (  LogUUID ,
          LogMessageLookupHash ,
          Depth ,
          StackTraceHash ) 
		SELECT logUUID ,
               msgLookupHash ,
               depth ,
               stackTraceHash
		FROM @logMsgs 
END

GO

/****** Object:  StoredProcedure [Logging].[spa_SaveLogs]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Stored Procedure

create PROCEDURE [Logging].[spa_SaveLogs]
/***********************************************************************************************************
*   Procedure name: [spa_SaveLogs]
*   Description:    save off data to log table for multiple logs
*					(R) AppLog 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@logs logging.appLogs READONLY

AS
BEGIN 

INSERT INTO logging.AppLog
        ( UUID ,
          ServiceInstanceHash ,
          SeverityLevelId ,
          CallingMethodName ,
          LogTimeStamp ,
          InFlightPayload
        )
		SELECT UUID ,
               ServiceInstanceHash ,
               SeverityLevelId ,
               CallingMethodName ,
               LogTimeStamp ,
               InFlightPayload
		FROM @logs

END



GO

/****** Object:  StoredProcedure [Logging].[spa_saveMessages]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Stored Procedure

CREATE PROCEDURE [Logging].[spa_saveMessages]
/***********************************************************************************************************
*   Procedure name: [spa_saveMessages]
*   Description:    save off message lookup entries
*					(R) MessageLookup 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@msgs logging.messages READONLY

AS
BEGIN 
 
WITH CTE AS(
   SELECT messageHash, MessageText,
       RN = ROW_NUMBER()OVER(PARTITION BY messageHash ORDER BY messageHash)
   FROM @msgs
) 
INSERT INTO logging.MessageLookup
        ( Hash, MessageText ) 
		SELECT messageHash ,
               messageText
		FROM CTE 
		WHERE NOT EXISTS 
			(SELECT TOP 1 * FROM logging.MessageLookup AS ml
				WHERE CTE.messageHash = ml.Hash)
				AND RN=1
END


GO

/****** Object:  StoredProcedure [Logging].[spa_saveStackTraces]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Logging].[spa_saveStackTraces]
/***********************************************************************************************************
*   Procedure name: [spa_saveLogMsg]
*   Description:    save off log messages
*					(R) LogMessages 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@stackTraces logging.stackTraces READONLY

AS
BEGIN 
	  
WITH CTE AS(
   SELECT stackTraceHash, stackText,
       RN = ROW_NUMBER()OVER(PARTITION BY stackTraceHash ORDER BY stackTraceHash)
   FROM @stackTraces
)
INSERT INTO logging.StackTraceLookup
        ( Hash, StackText ) 
SELECT stackTraceHash ,
               stackText
		FROM CTE 
		WHERE NOT EXISTS 
			(SELECT TOP 1 * FROM logging.StackTraceLookup AS stl
				WHERE CTE.stackTraceHash = stl.Hash)
			AND CTE.RN=1
END

GO

/****** Object:  StoredProcedure [Logging].[spa_SavingCallingMethodParamsIOs]    Script Date: 5/4/2017 9:25:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- Stored Procedure

CREATE PROCEDURE [Logging].[spa_SavingCallingMethodParamsIOs]
/***********************************************************************************************************
*   Procedure name: [spa_SavingCallingMethodParamsIOs]
*   Description:    save off data to calling method params
*					(R) CallingMethodIO 
*   Developer:      nick lawson
*   Creation Date:  5/11/2016
*
*   Change History: 
************************************************************************************************************/
@methodIOs logging.CallingMethodIOs READONLY

AS
BEGIN 

INSERT INTO logging.CallingMethodIO
        ( LogUUID, IsInput, ParameterName, Value ) 
		SELECT logUUID ,
               IsInput ,
			   parameterName,
               Value
		FROM @methodIOs 
END


GO


