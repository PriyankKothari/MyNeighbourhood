CREATE TABLE [dbo].[Member]
(
	[Id]			                    INT				NOT NULL PRIMARY KEY,
	[NeighbourhoodId]                   INT             NOT NULL,
	[FirstName]		                    VARCHAR(250)	NULL,
	[LastName]		                    VARCHAR(250)    NULL,
    [Gender]                            VARCHAR(10)     NULL,
    [MemberSince]                       DATETIME2       NULL,
    [Biography]                         VARCHAR(1000)   NULL,
    [FavouriteThingAboutNeighbourhood]  VARCHAR(1000)   NULL,
	[IsDeleted]		BIT				NULL,
	[CreatedOn]		DATETIME		NULL,
	[ModifiedOn]	DATETIME		NULL
)