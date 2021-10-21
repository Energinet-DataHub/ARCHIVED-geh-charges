CREATE TABLE [MessageHub].AvailableChargeData (
	Id uniqueidentifier not null primary key,

    AvailableDataReferenceId uniqueidentifier not null
);
GO

CREATE TABLE [MessageHub].AvailableChargeDataPoints (
    Id uniqueidentifier not null primary key,
    AvailableChargeDataId uniqueidentifier not null,
    Position int not null,
    Price decimal(14,6) not null
)
GO