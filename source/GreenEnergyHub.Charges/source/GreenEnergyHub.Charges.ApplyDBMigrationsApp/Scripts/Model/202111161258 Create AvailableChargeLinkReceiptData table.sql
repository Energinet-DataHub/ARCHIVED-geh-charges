CREATE TABLE [MessageHub].AvailableChargeLinkReceiptData (
    Id uniqueidentifier not null,
    RecipientId nvarchar(35) not null,
    RecipientRole int not null,
    BusinessReasonCode int not null,
    ReceiptStatus int not null,
    OriginalOperationId nvarchar(35) not null,
    MeteringPointId nvarchar(50) not null,
    RequestTime datetime2 not null,
    AvailableDataReferenceId uniqueidentifier not null,
    CONSTRAINT [PK_AvailableChargeLinkReceiptData] PRIMARY KEY NONCLUSTERED
        (
         ID ASC
            ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [MessageHub].AvailableChargeLinkReceiptDataReasonCode (
    Id uniqueidentifier not null,
    AvailableChargeLinkReceiptDataId uniqueidentifier not null FOREIGN KEY REFERENCES [MessageHub].AvailableChargeLinkReceiptData(Id),
    ReasonCode int not null,
    Text text not null
    CONSTRAINT [PK_AvailableChargeLinkReceiptDataReasonCodes] PRIMARY KEY NONCLUSTERED
            (
             ID ASC
                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX i1 ON [MessageHub].[AvailableChargeLinkReceiptDataReasonCode] (AvailableChargeLinkReceiptDataId);
GO