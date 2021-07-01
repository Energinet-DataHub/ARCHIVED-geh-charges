CREATE TABLE [Charges].[MeteringPoint]
(
    RowId bigint NOT NULL IDENTITY PRIMARY KEY,
    MeteringPointId NVARCHAR(50) NOT NULL,
    MeteringPointType smallint NOT NULL,
    MeteringGridArea NVARCHAR(30) NOT NULL,
    EffectiveDate DateTime NOT NULL,
    ConnectionState smallint NOT NULL,
    SettlementMethod smallint NOT NULL,
)
GO

CREATE INDEX i1 ON [Charges].[MeteringPoint] (RowId DESC);
GO