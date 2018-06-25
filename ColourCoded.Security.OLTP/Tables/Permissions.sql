CREATE TABLE [dbo].[Permissions]
(
  [PermissionId] INT IDENTITY(1,1) PRIMARY KEY,
  [ArtifactId] INT NOT NULL,
  [RoleId] INT NOT NULL,
  [CreateDate] DATETIME2 NOT NULL,
  [CreateUser] VARCHAR(255) NOT NULL
)
