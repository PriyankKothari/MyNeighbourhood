CREATE TABLE [geographic].[State]
(
	[Id]			INT				NOT NULL PRIMARY KEY,
	[CountryId]     INT             NOT NULL,
	[Name]			VARCHAR(250)	NULL,    
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)