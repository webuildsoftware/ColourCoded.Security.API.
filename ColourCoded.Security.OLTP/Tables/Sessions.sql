CREATE TABLE [dbo].[Sessions]
(
  [SessionId] INT IDENTITY(1,1) PRIMARY KEY,
  [Username] VARCHAR(255) NOT NULL,
  [Token] VARCHAR(255) NOT NULL,
  [MethodName] VARCHAR(244) NOT NULL,
  [Browser] VARCHAR(255) NULL,
  [Device] VARCHAR(255) NULL,
  [CreateDate] DATETIME2 NOT NULL,
  [ExpirationDate] DATETIME2 NOT NULL,
)
