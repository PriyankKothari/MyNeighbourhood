CREATE TABLE [dbo].[District]
(
	[Id]			INT				NOT NULL PRIMARY KEY,
	[StateId]		INT				NOT NULL,
	[Name]			VARCHAR(250)	NULL,	
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)