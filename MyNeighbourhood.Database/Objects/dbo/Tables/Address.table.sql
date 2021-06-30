CREATE TABLE [dbo].[Address]
(
	[Id]			    INT				NOT NULL PRIMARY KEY,
    [MemberId]          INT             NOT NULL,
    [AddressTypeId]     INT             NULL,
    [IsPreferred]       BIT             NULL,
    [AddressLineOne]    VARCHAR(250)    NULL,
    [AddressLineTwo]    VARCHAR(250)    NULL,
    [AddressLineThree]  VARCHAR(250)    NULL,
    [PostCode]          VARCHAR(10)     NULL,
    [Latitude]          NUMERIC(12, 9)  NULL,
    [Longitude]         NUMERIC(12, 9)  NULL,
    [NeighbourhoodId]   INT             NULL,
    [SuburbId]          INT             NULL,
    [CityId]            INT             NULL,
    [DistrictiId]       INT             NULL,
    [StateId]           INT             NULL,
    [CountryId]         INT             NULL,
	[IsDeleted]		    BIT				NULL,
	[CreatedOn]		    DATETIME		NULL,
	[ModifiedOn]	    DATETIME		NULL
)