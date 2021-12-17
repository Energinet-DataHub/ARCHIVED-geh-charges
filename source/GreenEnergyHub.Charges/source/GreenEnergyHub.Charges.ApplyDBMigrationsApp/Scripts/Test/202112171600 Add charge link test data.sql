DECLARE @chargeOwnerId UNIQUEIDENTIFIER
DECLARE @chargeId UNIQUEIDENTIFIER
DECLARE @meteringPointId VARCHAR(50)

SELECT @chargeOwnerId = Id FROM charges.marketparticipant WHERE marketparticipantid = '8100000000016'
SELECT @chargeId = NEWID()

INSERT INTO charges.charge VALUES (@chargeId, '40000', 3, @chargeOwnerId, 0, 2, 0, 'Description...', 'Transmissions nettarif', 1, '1999-12-31 23:00:00', NULL)

SELECT @meteringPointId = Id FROM charges.meteringpoint WHERE meteringpointid = '571313180000000005'

INSERT INTO charges.chargelink VALUES (newid(), @chargeId, @meteringPointId, '2019-12-31 23:00:00', '2021-12-31 23:00:00', 1)
