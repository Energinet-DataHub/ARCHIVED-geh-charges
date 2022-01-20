DECLARE @chargeOwnerId UNIQUEIDENTIFIER
DECLARE @chargeId UNIQUEIDENTIFIER
DECLARE @chargeId2 UNIQUEIDENTIFIER
DECLARE @chargeId3 UNIQUEIDENTIFIER
DECLARE @chargeId4 UNIQUEIDENTIFIER
DECLARE @meteringPointId VARCHAR(50)

SELECT @chargeOwnerId = Id FROM charges.marketparticipant WHERE marketparticipantid = '8100000000016'
SELECT @chargeId = NEWID()
SELECT @chargeId2 = NEWID()
SELECT @chargeId3 = NEWID()
SELECT @chargeId4 = NEWID()

INSERT INTO charges.charge VALUES (@chargeId, 'TestTariff', 3, @chargeOwnerId, 0, 2, 0, 'Description...', 'Grid Access Provider test tariff', 1, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId2, 'TestTar002', 3, @chargeOwnerId, 0, 2, 1, 'Description...', 'Grid Access Provider test tariff2', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId3, 'TestFee', 2, @chargeOwnerId, 0, 3, 0, 'Description...', 'Grid Access Provider test fee', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')
INSERT INTO charges.charge VALUES (@chargeId4, 'TestSub', 1, @chargeOwnerId, 0, 4, 0, 'Description...', 'Grid Access Provider test subscription', 2, '1999-12-31 23:00:00', '9999-12-31 23:59:59')

SELECT @meteringPointId = Id FROM charges.meteringpoint WHERE meteringpointid = '571313180000000005'

INSERT INTO charges.chargelink VALUES (newid(), @chargeId, @meteringPointId, '2019-12-31 23:00:00', '2022-12-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId2, @meteringPointId, '2020-12-31 23:00:00', '2021-12-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId2, @meteringPointId, '2021-12-31 23:00:00', '9999-12-31 23:59:59', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId3, @meteringPointId, '2022-01-30 23:00:00', '2022-01-31 23:00:00', 1)
INSERT INTO charges.chargelink VALUES (newid(), @chargeId4, @meteringPointId, '2022-03-31 22:00:00', '2022-12-31 23:00:00', 1)
