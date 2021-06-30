CREATE TABLE [geographic].[City]
(
	[Id]			INT				NOT NULL PRIMARY KEY,
	[DistrictId]	INT				NOT NULL,
	[Name]			VARCHAR(250)	NULL,	
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)