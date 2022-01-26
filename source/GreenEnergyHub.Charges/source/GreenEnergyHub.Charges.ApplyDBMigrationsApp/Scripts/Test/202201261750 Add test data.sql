------------------------------------------------------------------------------------------------------------------------
-- Add market participants
------------------------------------------------------------------------------------------------------------------------

DECLARE @provider8100000000016 UNIQUEIDENTIFIER = NEWID()
DECLARE @provider8100000000023 UNIQUEIDENTIFIER = NEWID()
DECLARE @provider8100000000030 UNIQUEIDENTIFIER = NEWID()
DECLARE @provider8900000000005 UNIQUEIDENTIFIER = NEWID()

INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000000108', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000000115', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000000122', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8510000000006', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8510000000013', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8510000000020', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8510000000037', 1, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (@provider8100000000016,'8100000000016', 2, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (@provider8100000000023,'8100000000023', 2, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (@provider8100000000030,'8100000000030', 2, 1);
INSERT INTO [Charges].[MarketParticipant] VALUES (@provider8900000000005,'8900000000005', 2, 0);
-- System operator has already been seeded in script 'Add default charges owner'
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'5790001330552', 7, 1);

------------------------------------------------------------------------------------------------------------------------
-- Add grid areas
------------------------------------------------------------------------------------------------------------------------

INSERT INTO [Charges].[GridArea] VALUES (NEWID(), 1, @provider8100000000016);
INSERT INTO [Charges].[GridArea] VALUES (NEWID(), 1, @provider8100000000023);
INSERT INTO [Charges].[GridArea] VALUES (NEWID(), 1, @provider8100000000030);
INSERT INTO [Charges].[GridArea] VALUES (NEWID(), 1, @provider8900000000005);

------------------------------------------------------------------------------------------------------------------------
-- Add grid area links
------------------------------------------------------------------------------------------------------------------------

DECLARE @areaIdOfProvider8100000000016 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaIdOfProvider8100000000023 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaIdOfProvider8100000000030 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaIdOfProvider8900000000005 UNIQUEIDENTIFIER = NEWID()

DECLARE @areaLinkIdOfProvider8100000000016 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaLinkIdOfProvider8100000000023 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaLinkIdOfProvider8100000000030 UNIQUEIDENTIFIER = NEWID()
DECLARE @areaLinkIdOfProvider8900000000005 UNIQUEIDENTIFIER = NEWID()

INSERT INTO [Charges].[GridAreaLink] VALUES (@areaLinkIdOfProvider8100000000016, @areaIdOfProvider8100000000016);
INSERT INTO [Charges].[GridAreaLink] VALUES (@areaLinkIdOfProvider8100000000023, @areaIdOfProvider8100000000023);
INSERT INTO [Charges].[GridAreaLink] VALUES (@areaLinkIdOfProvider8100000000030, @areaIdOfProvider8100000000030);
INSERT INTO [Charges].[GridAreaLink] VALUES (@areaLinkIdOfProvider8900000000005, @areaIdOfProvider8900000000005);

------------------------------------------------------------------------------------------------------------------------
-- Add metering points
------------------------------------------------------------------------------------------------------------------------

INSERT INTO [Charges].[MeteringPoint] VALUES (NEWID(), '571313180000000005', 1, @areaLinkIdOfProvider8100000000016, '2020-12-31T23:00:00', 2, 3)
INSERT INTO [Charges].[MeteringPoint] VALUES (NEWID(), '571313180000000012', 3, @areaLinkIdOfProvider8100000000023, '2020-12-31T23:00:00', 1, 3)
INSERT INTO [Charges].[MeteringPoint] VALUES (NEWID(), '571313180000000029', 1, @areaLinkIdOfProvider8100000000030, '2020-12-31T23:00:00', 1, 3)

------------------------------------------------------------------------------------------------------------------------
-- Add charges
------------------------------------------------------------------------------------------------------------------------

DECLARE @chargeOwnerId UNIQUEIDENTIFIER
DECLARE @chargeId UNIQUEIDENTIFIER = NEWID()
DECLARE @chargeId2 UNIQUEIDENTIFIER = NEWID()
DECLARE @chargeId3 UNIQUEIDENTIFIER = NEWID()
DECLARE @chargeId4 UNIQUEIDENTIFIER = NEWID()
DECLARE @meteringPointId VARCHAR(50)

SELECT @chargeOwnerId = Id FROM charges.marketparticipant WHERE marketparticipantid = '8100000000016'

INSERT INTO charges.charge VALUES (@chargeId, 'TestTariff', 3, @chargeOwnerId, 0, 2, 0, 'Description...', 'Grid Access Provider test tariff', 1, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId2, 'TestTar002', 3, @chargeOwnerId, 0, 2, 1, 'Description...', 'Grid Access Provider test tariff2', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId3, 'TestFee', 2, @chargeOwnerId, 0, 3, 0, 'Description...', 'Grid Access Provider test fee', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId4, 'TestSub', 1, @chargeOwnerId, 0, 4, 0, 'Description...', 'Grid Access Provider test subscription', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')

------------------------------------------------------------------------------------------------------------------------
-- Add charge links
------------------------------------------------------------------------------------------------------------------------

SELECT @meteringPointId = Id FROM charges.meteringpoint WHERE meteringpointid = '571313180000000005'

INSERT INTO charges.chargelink VALUES (newid(), @chargeId, @meteringPointId, '2019-12-31 23:00:00', '2022-12-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId2, @meteringPointId, '2020-12-31 23:00:00', '2021-12-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId2, @meteringPointId, '2021-12-31 23:00:00', '9999-12-31 23:59:59', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId3, @meteringPointId, '2022-01-30 23:00:00', '2022-01-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId4, @meteringPointId, '2022-03-31 22:00:00', '2022-12-31 23:00:00', 1)
