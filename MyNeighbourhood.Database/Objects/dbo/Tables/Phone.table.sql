CREATE TABLE [dbo].[Phone]
(
    [Id]			        INT				NOT NULL PRIMARY KEY,
    [ContactId]             INT             NOT NULL,
    [PhoneTypeId]           INT             NULL,
    [IsPreferred]           BIT             NULL,
    [CountryCode]           INT             NULL,
    [PhoneAreaCode]         INT             NULL,
    [PhoneNumber]           INT             NULL,
    [ExtensionNumber]       INT             NULL,
    [PhoneDescription]      VARCHAR(1000)   NULL,
    [IsDeleted]		        BIT				NULL,
	[CreatedOn]		        DATETIME		NULL,
	[ModifiedOn]	        DATETIME		NULL
)