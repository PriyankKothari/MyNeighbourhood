CREATE TABLE [geographic].[Neighbourhood]
(
    [Id]			INT				NOT NULL PRIMARY KEY,
	[SuburbId]		INT				NOT NULL,
	[Name]			VARCHAR(250)	NULL,
    [Description]   VARCHAR(1000)   NULL,	
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)