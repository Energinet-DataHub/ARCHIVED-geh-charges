DROP TABLE [Charges].[ChargeLinkPeriodDetails]
DROP TABLE [Charges].[ChargeLinkOperation]
DROP TABLE [Charges].[ChargeLink]

CREATE TABLE [Charges].[ChargeLink]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeId int NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(RowId),
    MeteringPointId int NOT NULL FOREIGN KEY REFERENCES [Charges].MeteringPoint(RowId)
        CONSTRAINT [PK_ChargeLink] PRIMARY KEY NONCLUSTERED
            (
             ID ASC
                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_MeteringPointId ON [Charges].[ChargeLink] (MeteringPointId DESC, ChargeId DESC);
GO

CREATE TABLE [Charges].[ChargeLinkOperation]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeLinkId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLink(Id),
    CorrelationId nvarchar(36) NOT NULL,
    CustomerProvidedId nvarchar(100) NOT NULL,
    WriteDateTime DateTime2 NOT NULL DEFAULT(GETDATE())
        CONSTRAINT [PK_ChargeLinkOperation] PRIMARY KEY NONCLUSTERED
            (
             ID ASC
                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_ChargeLinkId ON [Charges].[ChargeLinkOperation] (ChargeLinkId DESC, WriteDateTime DESC);
GO

CREATE TABLE [Charges].[ChargeLinkPeriodDetails]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeLinkId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLink(Id),
    StartDateTime DateTime2 NOT NULL,
    EndDateTime DateTime2 NOT NULL,
    Factor int NOT NULL,
    CreatedByOperationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeLinkOperation(Id),
    RetiredByOperationId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Charges].ChargeLinkOperation(Id)
        CONSTRAINT [PK_ChargeLinkPeriodDetails] PRIMARY KEY NONCLUSTERED
            (
             ID ASC
                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_ChargeLinkId_StartDateTime_EndDateTime ON [Charges].[ChargeLinkPeriodDetails] (ChargeLinkId DESC, StartDateTime DESC, EndDateTime DESC);
CREATE INDEX IX_ChargeLinkOperationId ON [Charges].[ChargeLinkPeriodDetails] (CreatedByOperationId ASC);
CREATE INDEX IX_RetiredByOperationId ON [Charges].[ChargeLinkPeriodDetails] (RetiredByOperationId ASC);
GO
