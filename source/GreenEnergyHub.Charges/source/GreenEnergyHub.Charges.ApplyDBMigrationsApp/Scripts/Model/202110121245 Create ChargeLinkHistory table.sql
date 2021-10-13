CREATE TABLE [Charges].ChargeLinkHistory (
	Id uniqueidentifier not null primary key,
    Recipient varchar(50) not null,
    MarketParticipantRole int not null,
    BusinessReasonCode int not null,
    ChargeId varchar(50) not null,
    MeteringPointId varchar(50) not null,
    Owner varchar(50) not null,
    Factor int not null,
    ChargeType varchar(50) not null,
    ValidFrom datetime2 not null,
    ValidTo datetime2 not null,
    PostOfficeId uniqueidentifier not null
);
GO