CREATE TABLE [Charges].ChargeLinkHistory (
	Id uniqueidentifier not null primary key,
    Recipient varchar(70) not null,
    MarketParticipantRole int not null,
    BusinessReasonCode int not null,
    ChargeId varchar(70) not null,
    MeteringPointId varchar(70) not null,
    Owner varchar(70) not null,
    Factor int not null,
    ChargeType int not null,
    ValidFrom datetime2 not null,
    ValidTo datetime2 not null,
    MessageHubId uniqueidentifier not null
);
GO