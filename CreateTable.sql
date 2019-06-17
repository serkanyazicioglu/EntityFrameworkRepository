/****** Object:  Table [dbo].[Member]    Script Date: 17.06.2019 18:13:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Member](
	[Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](200) NULL,
	[UserName] [nvarchar](200) NULL,
	[Password] [nvarchar](100) NULL,
	[CreateDate] [datetime] NULL,
	[Status] [int] NULL,
	[Email] [nvarchar](1000) NULL,
	[Type] [int] NULL,
	[ProfilePictureUrl] [nvarchar](500) NULL,
	[PasswordRecoveryId] [uniqueidentifier] NULL,
	[HashSalt] [varchar](32) NULL

 CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Member] ADD  CONSTRAINT [DF_Member_Id]  DEFAULT (newid()) FOR [Id]
GO