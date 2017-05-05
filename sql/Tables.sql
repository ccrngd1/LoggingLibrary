USE [ClaimsAuditWarehouse]
GO

/****** Object:  Table [Logging].[AdditionalData]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[AdditionalData](
	[LogUUID] [uniqueidentifier] NOT NULL,
	[KeyName] [varchar](255) NOT NULL,
	[Value] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [Logging].[AppLog]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[AppLog](
	[UUID] [uniqueidentifier] NOT NULL,
	[ServiceInstanceHash] [varchar](64) NOT NULL,
	[SeverityLevelId] [int] NOT NULL,
	[CallingMethodName] [varchar](100) NULL,
	[InFlightPayload] [varchar](max) NULL,
	[LogTimeStamp] [datetime] NOT NULL,
 CONSTRAINT [pk_loggingAppLogUUID] PRIMARY KEY NONCLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [Logging].[CallingMethodIO]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[CallingMethodIO](
	[LogUUID] [uniqueidentifier] NOT NULL,
	[IsInput] [bit] NOT NULL,
	[ParameterName] [varchar](255) NOT NULL,
	[Value] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [Logging].[LogMessages]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[LogMessages](
	[LogUUID] [uniqueidentifier] NOT NULL,
	[LogMessageLookupHash] [char](64) NOT NULL,
	[Depth] [int] NOT NULL,
	[StackTraceHash] [char](64) NULL
) ON [PRIMARY]

GO

/****** Object:  Table [Logging].[LogServiceInstances]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[LogServiceInstances](
	[LogServiceInstanceHash] [char](64) NOT NULL,
	[UniversalAppPath] [varchar](max) NOT NULL,
	[LogServiceHash] [char](64) NOT NULL,
	[AppVersion] [varchar](15) NOT NULL,
 CONSTRAINT [pk_LogServiceInsdtId] PRIMARY KEY NONCLUSTERED 
(
	[LogServiceInstanceHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [Logging].[LogServices]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[LogServices](
	[ServiceHash] [char](64) NOT NULL,
	[AppName] [varchar](255) NOT NULL,
 CONSTRAINT [pk_LogServiceId] PRIMARY KEY NONCLUSTERED 
(
	[ServiceHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [Logging].[MessageLookup]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[MessageLookup](
	[Hash] [CHAR](64) NOT NULL,
	[MessageText] [VARCHAR](MAX) NOT NULL,
 CONSTRAINT [pk_MessageLookupHash] PRIMARY KEY NONCLUSTERED 
(
	[Hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [Logging].[SeverityLevel]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[SeverityLevel](
	[Id] [INT] NOT NULL,
	[Name] [VARCHAR](255) NOT NULL,
 CONSTRAINT [PK_Logging.SeverityLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [Logging].[StackTraceLookup]    Script Date: 5/4/2017 9:22:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Logging].[StackTraceLookup](
	[Hash] [CHAR](64) NOT NULL,
	[StackText] [TEXT] NOT NULL,
 CONSTRAINT [pk_StackTraceHash] PRIMARY KEY NONCLUSTERED 
(
	[Hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


