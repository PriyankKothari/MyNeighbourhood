CREATE TABLE [geographic].[Country]
(
	[Id]					INT				NOT NULL PRIMARY KEY,
	[Name]					VARCHAR(100)	NULL,
	[Code]					VARCHAR(10)		NULL,
	[Captial]				VARCHAR(250)	NULL,
	[Currency]				VARCHAR(250)	NULL,
	[PhoneCode]				VARCHAR(250)	NULL,	
    [Latitude]				NUMERIC(12, 9)  NULL,
    [Longitude]				NUMERIC(12, 9)  NULL,
	[WikipediaSource]		VARCHAR(250)	NULL,
	[IsDeleted]				BIT				NULL,
	[CreatedOn]				DATETIME		NULL,
	[ModifiedOn]			DATETIME		NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_Country_Code] ON [geographic].[country] ([Code])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]