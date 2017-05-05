USE [ClaimsAuditWarehouse]
GO

/****** Object:  UserDefinedTableType [Logging].[AdditionalDatas]    Script Date: 5/4/2017 9:28:50 PM ******/
CREATE TYPE [Logging].[AdditionalDatas] AS TABLE(
	[logUUID] [uniqueidentifier] NOT NULL,
	[KeyName] [varchar](255) NOT NULL,
	[Value] [varchar](max) NOT NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[AppLogs]    Script Date: 5/4/2017 9:28:50 PM ******/
CREATE TYPE [Logging].[AppLogs] AS TABLE(
	[UUID] [uniqueidentifier] NOT NULL,
	[ServiceInstanceHash] [varchar](64) NOT NULL,
	[SeverityLevelId] [int] NOT NULL,
	[CallingMethodName] [varchar](255) NULL,
	[LogTimeStamp] [datetime] NOT NULL,
	[InFlightPayload] [varchar](max) NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[CallingMethodIOs]    Script Date: 5/4/2017 9:28:50 PM ******/
CREATE TYPE [Logging].[CallingMethodIOs] AS TABLE(
	[logUUID] [UNIQUEIDENTIFIER] NOT NULL,
	[IsInput] [BIT] NOT NULL,
	[parameterName] [VARCHAR](255) NOT NULL,
	[Value] [VARCHAR](MAX) NOT NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[LogMessages]    Script Date: 5/4/2017 9:28:51 PM ******/
CREATE TYPE [Logging].[LogMessages] AS TABLE(
	[logUUID] [UNIQUEIDENTIFIER] NOT NULL,
	[msgLookupHash] [VARCHAR](64) NOT NULL,
	[depth] [INT] NOT NULL,
	[stackTraceHash] [VARCHAR](64) NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[logServices]    Script Date: 5/4/2017 9:28:51 PM ******/
CREATE TYPE [Logging].[logServices] AS TABLE(
	[logSvcHash] [VARCHAR](64) NOT NULL,
	[appName] [VARCHAR](255) NOT NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[logServicesInstances]    Script Date: 5/4/2017 9:28:51 PM ******/
CREATE TYPE [Logging].[logServicesInstances] AS TABLE(
	[logSvcInstanceHash] [VARCHAR](64) NOT NULL,
	[appPath] [VARCHAR](MAX) NOT NULL,
	[logSvcHash] [VARCHAR](64) NOT NULL,
	[appVersion] [VARCHAR](10) NOT NULL,
	[buildDate] [DATETIME] NOT NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[messages]    Script Date: 5/4/2017 9:28:51 PM ******/
CREATE TYPE [Logging].[messages] AS TABLE(
	[messageHash] [VARCHAR](64) NOT NULL,
	[messageText] [VARCHAR](MAX) NOT NULL
)
GO

/****** Object:  UserDefinedTableType [Logging].[stackTraces]    Script Date: 5/4/2017 9:28:51 PM ******/
CREATE TYPE [Logging].[stackTraces] AS TABLE(
	[stackTraceHash] [VARCHAR](64) NOT NULL,
	[stackText] [TEXT] NOT NULL
)
GO


