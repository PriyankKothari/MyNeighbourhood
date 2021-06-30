CREATE TABLE [dbo].[Contact]
(
    [Id]			        INT				NOT NULL PRIMARY KEY,
    [MemberId]              INT             NOT NULL,
    [ContactTypeId]         INT             NULL,
    [FacebookProfileLink]   VARCHAR(250)    NULL,
    [TwitterHandle]         VARCHAR(250)    NULL,
    [IsDeleted]		        BIT				NULL,
	[CreatedOn]		        DATETIME		NULL,
	[ModifiedOn]	        DATETIME		NULL
)