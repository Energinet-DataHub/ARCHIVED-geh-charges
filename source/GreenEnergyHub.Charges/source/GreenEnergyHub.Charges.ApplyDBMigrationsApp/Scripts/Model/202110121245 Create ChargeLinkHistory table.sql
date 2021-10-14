CREATE TABLE [Charges].ChargeLinkHistory (
	Id uniqueidentifier not null primary key,
    Recipient varchar(70) not null,
    RecipientRole int not null,
    BusinessReasonCode int not null,
    ChargeId varchar(70) not null,
    ChargeOwner varchar(70) not null,
    ChargeType int not null,
    MeteringPointId varchar(70) not null,
    Factor int not null,
    StartDateTime datetime2 not null,
    EndDateTime datetime2 not null,
    MessageHubId uniqueidentifier not null
);
GO