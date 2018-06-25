CREATE TABLE [dbo].[Artifacts]
(
  [ArtifactId] INT IDENTITY(1,1) PRIMARY KEY,
  [ArtifactName] VARCHAR(255) NOT NULL,
  [CreateDate] DATETIME2 NOT NULL,
  [CreateUser] VARCHAR(255) NOT NULL,
  [UpdateDate] DATETIME2 NULL,
  [UpdateUser] VARCHAR(255) NULL,
)
