CREATE TABLE [Charges].[ChargeLink]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(RowId),
    MeteringPointRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].MeteringPoint(RowId)
    )
    GO

CREATE INDEX i1 ON [Charges].[ChargeLink] (MeteringPointRowId DESC, ChargeRowId DESC);
GO

CREATE TABLE [Charges].[ChargeLinkOperation]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeLinkRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLink(RowId),
    CorrelationId nvarchar(36) NOT NULL,
    ChargeLinkOperationId nvarchar(100) NOT NULL,
    WriteDateTime DateTime2 NOT NULL
    )
    GO

CREATE INDEX i1 ON [Charges].[ChargeLinkOperation] (ChargeLinkRowId DESC, WriteDateTime DESC);
GO

CREATE TABLE [Charges].[ChargeLinkPeriodDetails]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeLinkRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLink(RowId),
    StartDateTime DateTime2 NOT NULL,
    EndDateTime DateTime2,
    Factor int NOT NULL,
    ChargeLinkOperationRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLinkOperation(RowId),
    RetiredByChargeLinkOperationRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLinkOperation(RowId)
    )
    GO

CREATE INDEX i1 ON [Charges].[ChargeLinkPeriodDetails] (ChargeLinkRowId DESC, StartDateTime DESC, EndDateTime DESC);
CREATE INDEX i2 ON [Charges].[ChargeLinkPeriodDetails] (ChargeLinkOperationRowId ASC);
CREATE INDEX i3 ON [Charges].[ChargeLinkPeriodDetails] (RetiredByChargeLinkOperationRowId ASC);
GO
