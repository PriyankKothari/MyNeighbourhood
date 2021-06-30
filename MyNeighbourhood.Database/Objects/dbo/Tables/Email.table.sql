CREATE TABLE [dbo].[Email]
(
	[Id]			        INT				NOT NULL PRIMARY KEY,
    [ContactId]             INT             NOT NULL,
    [EmailTypeId]           INT             NULL,
    [IsPreferred]           BIT             NULL,
    [EmailAddress]          VARCHAR(250)    NULL,
    [EmailDescription]      VARCHAR(1000)   NULL,
    [IsDeleted]		        BIT				NULL,
	[CreatedOn]		        DATETIME		NULL,
	[ModifiedOn]	        DATETIME		NULL
)