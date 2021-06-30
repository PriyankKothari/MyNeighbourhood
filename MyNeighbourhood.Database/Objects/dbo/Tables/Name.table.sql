CREATE TABLE [dbo].[Name]
(
	[Id]			        INT				NOT NULL PRIMARY KEY,
    [ContactId]             INT             NOT NULL,
    [NameTypeId]            INT             NULL,
    [IsPreferred]           BIT             NULL,
    [TitleTypeId]           INT             NULL,
    [FirstName]             VARCHAR(250)    NULL,
    [MiddleName]            VARCHAR(250)    NULL,
    [LastName]              VARCHAR(250)    NULL,
    [IsDeleted]		        BIT				NULL,
	[CreatedOn]		        DATETIME		NULL,
	[ModifiedOn]	        DATETIME		NULL
)