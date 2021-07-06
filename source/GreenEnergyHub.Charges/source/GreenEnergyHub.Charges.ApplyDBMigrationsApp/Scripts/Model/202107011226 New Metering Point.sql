CREATE TABLE [Charges].[MeteringPoint]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    MeteringPointId NVARCHAR(50) NOT NULL,
    MeteringPointType int NOT NULL,
    MeteringGridArea NVARCHAR(50) NOT NULL,
    EffectiveDate DateTime NOT NULL,
    ConnectionState int NOT NULL,
    SettlementMethod int NOT NULL,
)
GO

CREATE INDEX i2 ON [Charges].[MeteringPoint] (MeteringPointId DESC);
GO