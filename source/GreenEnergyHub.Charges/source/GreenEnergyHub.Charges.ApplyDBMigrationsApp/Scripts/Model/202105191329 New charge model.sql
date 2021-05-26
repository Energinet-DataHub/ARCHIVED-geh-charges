CREATE SCHEMA Charges;
GO

CREATE TABLE [Charges].[MarketParticipant]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    MarketParticipantId nvarchar(35) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Role int NOT NULL
)
GO

CREATE INDEX i1 ON [Charges].[MarketParticipant] (MarketParticipantId);
GO

CREATE TABLE [Charges].[Charge]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeId nvarchar(35) NOT NULL,
    ChargeType int NOT NULL,
    Owner int NOT NULL FOREIGN KEY REFERENCES [Charges].MarketParticipant(RowId),
    TaxIndicator bit NOT NULL,
    Resolution int NOT NULL,
    Currency nvarchar(10) NOT NULL,
    TransparentInvoicing bit NOT NULL
)
GO

CREATE UNIQUE INDEX i1 ON [Charges].[Charge] (ChargeId ASC, ChargeType ASC, Owner ASC);
GO

CREATE TABLE [Charges].[ChargeOperation]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(RowId),
    CorrelationId nvarchar(36) NOT NULL,
    ChargeOperationId nvarchar(100) NOT NULL,
    WriteDateTime DateTime NOT NULL
)
GO

CREATE UNIQUE INDEX i1 ON [Charges].[ChargeOperation] (ChargeRowId DESC, WriteDateTime DESC);
GO

CREATE TABLE [Charges].[ChargePrice]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(RowId),
    Time DateTime NOT NULL,
    Price decimal(14,6) NOT NULL,
    Retired bit NOT NULL,
    RetiredDateTime DateTime,
    ChargeOperationRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeOperation(RowId)
)
GO

CREATE UNIQUE INDEX i1 ON [Charges].[ChargePrice] (ChargeRowId DESC, Time DESC);
CREATE INDEX i2 ON [Charges].[ChargePrice] (ChargeOperationRowId);
GO

CREATE TABLE [Charges].[ChargePeriodDetails]
(
    RowId int NOT NULL IDENTITY PRIMARY KEY,
    ChargeRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(RowId),
    StartDateTime DateTime NOT NULL,
    EndDateTime DateTime,
    Name nvarchar(132) NOT NULL,
    Description nvarchar(2048) NOT NULL,
    VatClassification int NOT NULL,
    Retired bit NOT NULL,
    RetiredDateTime DateTime,
    ChargeOperationRowId int NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeOperation(RowId)
)
GO

CREATE UNIQUE INDEX i1 ON [Charges].[ChargePeriodDetails] (ChargeRowId DESC, StartDateTime DESC, EndDateTime DESC, Retired DESC);
CREATE INDEX i2 ON [Charges].[ChargePeriodDetails] (ChargeOperationRowId);
GO