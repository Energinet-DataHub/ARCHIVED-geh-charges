CREATE TABLE [MessageHub].AvailableChargeData (
	Id uniqueidentifier not null primary key,

    AvailableDataReferenceId uniqueidentifier not null,
    TaxIndicator bit not null,
    TransparentInvoicing bit not null,
    VatClassification int not null,
    RequestTime datetime2 not null,
);
GO

CREATE TABLE [MessageHub].AvailableChargeDataPoints (
    Id uniqueidentifier not null primary key,
    AvailableChargeDataId uniqueidentifier not null,
    Position int not null,
    Price decimal(14,6) not null
)
GO