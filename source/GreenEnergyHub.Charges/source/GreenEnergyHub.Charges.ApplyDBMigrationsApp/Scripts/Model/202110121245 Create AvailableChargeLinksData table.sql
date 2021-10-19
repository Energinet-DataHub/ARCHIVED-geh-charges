CREATE SCHEMA MessageHub;
GO

CREATE TABLE [MessageHub].AvailableChargeLinksData (
	Id uniqueidentifier not null primary key,
    RecipientId varchar(70) not null,
    RecipientRole int not null,
    BusinessReasonCode int not null,
    ChargeId varchar(70) not null,
    ChargeOwner varchar(70) not null,
    ChargeType int not null,
    MeteringPointId varchar(70) not null,
    Factor int not null,
    StartDateTime datetime2 not null,
    EndDateTime datetime2 not null,
    AvailableDataReferenceId uniqueidentifier not null
);
GO