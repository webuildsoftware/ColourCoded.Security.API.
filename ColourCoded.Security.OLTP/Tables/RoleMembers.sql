CREATE TABLE [dbo].[RoleMembers]
(
  [RoleMemberId] INT IDENTITY(1,1) PRIMARY KEY,
  [RoleId] INT NOT NULL,
  [Username] VARCHAR(255) NOT NULL,
  [CreateDate] DATETIME2 NOT NULL,
  [CreateUser] VARCHAR(255) NOT NULL,
  [UpdateDate] DATETIME2 NULL,
  [UpdateUser] VARCHAR(255) NULL,
)
