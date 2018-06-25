CREATE TABLE [dbo].[Users]
(
  [UserId] INT IDENTITY(1,1) PRIMARY KEY,
  [Username] VARCHAR(255),
  [Password] VARCHAR(510),
  [EmailAddress] VARCHAR(255),
  [FirstName] VARCHAR(255),
  [LastName] VARCHAR(255),
  [CompanyProfileId] [int] default(0) NOT NULL,
  [CreateDate] DATETIME2,
  [CreateUser] VARCHAR(255),
  [UpdateDate] DATETIME2 NULL,
  [UpdateUser] VARCHAR(255) NULL,
)
