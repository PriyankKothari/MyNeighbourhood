CREATE TABLE [dbo].[Website]
(
    [Id]			        INT				NOT NULL PRIMARY KEY,
    [ContactId]             INT             NOT NULL,
    [WebsiteTypeId]         INT             NULL,
    [IsPreferred]           BIT             NULL,
    [WebsiteAddress]        VARCHAR(250)    NULL,
    [WebsiteDescription]    VARCHAR(1000)   NULL,
    [IsDeleted]		        BIT				NULL,
	[CreatedOn]		        DATETIME		NULL,
	[ModifiedOn]	        DATETIME		NULL
)