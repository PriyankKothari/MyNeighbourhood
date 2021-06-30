CREATE TABLE [geographic].[Suburb]
(
	[Id]			INT				NOT NULL PRIMARY KEY,
	[Name]			VARCHAR(250)	NOT NULL,
	[CityId]		INT				NULL,
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)