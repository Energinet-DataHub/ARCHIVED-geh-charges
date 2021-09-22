DROP TABLE [Charges].[ChargeLinkPeriodDetails]
DROP TABLE [Charges].[ChargeLinkOperation]
DROP TABLE [Charges].[ChargeLink]
DROP TABLE [Charges].[MeteringPoint]
DROP TABLE [Charges].[DefaultChargeLink]
DROP TABLE [Charges].[ChargePeriodDetails]
DROP TABLE [Charges].[ChargePrice]
DROP TABLE [Charges].[ChargeOperation]
DROP TABLE [Charges].[Charge]
DROP TABLE [Charges].[MarketParticipant]

CREATE TABLE [Charges].[MarketParticipant]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    MarketParticipantId nvarchar(35) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Role int NOT NULL
    CONSTRAINT [PK_MarketParticipant] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE TABLE [Charges].[Charge]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    SenderProvidedChargeId nvarchar(100) NOT NULL,
    ChargeType int NOT NULL,
    MarketParticipantId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].MarketParticipant(Id),
    TaxIndicator bit NOT NULL,
    Resolution int NOT NULL,
    Currency nvarchar(10) NOT NULL,
    TransparentInvoicing bit NOT NULL
    CONSTRAINT [PK_Charge] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE INDEX IX_SenderProvidedId_ChargeType_MarketParticipantId ON [Charges].[Charge] (SenderProvidedChargeId ASC, ChargeType ASC, MarketParticipantId ASC);
GO

CREATE TABLE [Charges].[ChargeOperation]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(Id),
    CorrelationId nvarchar(36) NOT NULL,
    ChargeOperationId nvarchar(100) NOT NULL,
    WriteDateTime DateTime2 NOT NULL
    CONSTRAINT [PK_ChargeOperation] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE INDEX IX_ChargeOpertionId ON [Charges].[ChargeOperation] (ChargeId DESC, WriteDateTime DESC);
GO

CREATE TABLE [Charges].[ChargePeriodDetails]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(Id),
    StartDateTime DateTime2 NOT NULL,
    EndDateTime DateTime2,
    Name nvarchar(132) NOT NULL,
    Description nvarchar(2048) NOT NULL,
    VatClassification int NOT NULL,
    Retired bit NOT NULL,
    RetiredDateTime DateTime2,
    ChargeOperationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeOperation(Id)
    CONSTRAINT [PK_ChargePeriodDetails] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE INDEX IX_ChargeId_StartDateTime_EndDateTime_Retired ON [Charges].[ChargePeriodDetails] (ChargeId DESC, StartDateTime DESC, EndDateTime DESC, Retired DESC);
CREATE INDEX IX_ChargeOperationId ON [Charges].[ChargePeriodDetails] (ChargeOperationId);
GO

CREATE TABLE [Charges].[ChargePrice]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(Id),
    Time DateTime2 NOT NULL,
    Price decimal(14,6) NOT NULL,
    Retired bit NOT NULL,
    RetiredDateTime DateTime2,
    ChargeOperationId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].ChargeOperation(Id)
    CONSTRAINT [PK_ChargePrice] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE TABLE [Charges].[DefaultChargeLink]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    MeteringPointType int NOT NULL,
    ChargeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(Id),
    StartDateTime DateTime2 NOT NULL,
    EndDateTime DateTime2,
    CONSTRAINT [PK_DefaultChargeLink] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE INDEX IX_MeteringPointType_StartDateTime_EndDateTime ON [Charges].[DefaultChargeLink] (MeteringPointType ASC, StartDateTime DESC, EndDateTime DESC);
GO

CREATE TABLE [Charges].[MeteringPoint]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    MeteringPointId NVARCHAR(50) NOT NULL,
    MeteringPointType int NOT NULL,
    GridAreaId NVARCHAR(50) NOT NULL,
    EffectiveDate DateTime2 NOT NULL,
    ConnectionState int NOT NULL,
    SettlementMethod int,
    CONSTRAINT [PK_MeteringPoint] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO


CREATE TABLE [Charges].[ChargeLink]
(
    Id UNIQUEIDENTIFIER NOT NULL,
    ChargeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].Charge(Id),
    MeteringPointId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Charges].MeteringPoint(Id)
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
    SenderProvidedId nvarchar(100) NOT NULL,
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