CREATE TABLE [MessageHub].AvailableChargeData (
    Id uniqueidentifier not null,
    RecipientId nvarchar(35) not null,
    RecipientRole int not null,
    BusinessReasonCode int not null,
    ChargeId nvarchar(35) not null,
    ChargeOwner nvarchar(35) not null,
    ChargeType int not null,
    ChargeName nvarchar(132) not null,
    ChargeDescription nvarchar(2048) not null,
    StartDateTime datetime2 not null,
    EndDateTime datetime2 not null,
    TaxIndicator bit not null,
    TransparentInvoicing bit not null,
    VatClassification int not null,
    Resolution int not null,
    RequestTime datetime2 not null,
    AvailableDataReferenceId uniqueidentifier not null,
    CONSTRAINT [PK_AvailableChargeData] PRIMARY KEY NONCLUSTERED
        (
         ID ASC
            ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [MessageHub].AvailableChargeDataPoints (
    Id uniqueidentifier not null,
    AvailableChargeDataId uniqueidentifier not null FOREIGN KEY REFERENCES [MessageHub].AvailableChargeData(Id),
    Position int not null,
    Price decimal(14,6) not null
        CONSTRAINT [PK_AvailableChargeDataPoints] PRIMARY KEY NONCLUSTERED
            (
             ID ASC
                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO