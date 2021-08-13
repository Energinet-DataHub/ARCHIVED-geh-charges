CREATE TABLE [Charges].[DefaultChargeLinkSetting]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    MeteringPointType int NOT NULL,
    ChargeRowId int NOT NULL,
    StartDateTime DateTime NOT NULL,
    EndDateTime DateTime,
)
GO

CREATE INDEX i1 ON [Charges].[DefaultChargeLinkSetting] (MeteringPointType ASC, StartDateTime DESC, EndDateTime DESC);
GO