CREATE TABLE [dbo].[Timezone]
(
	[Id]					INT				NOT NULL PRIMARY KEY,
	[CountryId]				INT				NOT NULL,
	[DisplayName]			VARCHAR(250)	NULL,
	[StandardName]			VARCHAR(250)	NULL,
	[HasDayLightSavingTime]	BIT				NULL,
	[UTCOffSet]				INT				NULL,
	[IsDeleted]				BIT				NULL,
	[CreatedOn]				DATETIME		NULL,
	[ModifiedOn]			DATETIME		NULL
)