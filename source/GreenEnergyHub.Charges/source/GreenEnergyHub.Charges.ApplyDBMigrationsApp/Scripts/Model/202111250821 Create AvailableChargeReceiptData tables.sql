CREATE TABLE [MessageHub].AvailableChargeReceiptData (
                                                         Id uniqueidentifier not null,
                                                         RecipientId nvarchar(35) not null,
    RecipientRole int not null,
    BusinessReasonCode int not null,
    ReceiptStatus int not null,
    OriginalOperationId nvarchar(35) not null,
    RequestDateTime datetime2 not null,
    AvailableDataReferenceId uniqueidentifier not null,
    CONSTRAINT [PK_AvailableChargeReceiptData] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE TABLE [MessageHub].AvailableChargeReceiptDataReasonCode (
                                                                   Id uniqueidentifier not null,
                                                                   AvailableChargeReceiptDataId uniqueidentifier not null FOREIGN KEY REFERENCES [MessageHub].AvailableChargeReceiptData(Id),
    ReasonCode int not null,
    Text nvarchar(MAX) not null
    CONSTRAINT [PK_AvailableChargeReceiptDataReasonCodes] PRIMARY KEY NONCLUSTERED
(
    ID ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

CREATE NONCLUSTERED INDEX i1 ON [MessageHub].[AvailableChargeReceiptDataReasonCode] (AvailableChargeReceiptDataId);
GO