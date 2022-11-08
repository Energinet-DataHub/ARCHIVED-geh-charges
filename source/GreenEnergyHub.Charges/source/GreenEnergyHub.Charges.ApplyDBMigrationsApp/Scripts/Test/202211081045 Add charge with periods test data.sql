------------------------------------------------------------------------------------------------------------------------
-- Add charge with multiple periods
------------------------------------------------------------------------------------------------------------------------

DECLARE @chargeOwnerId UNIQUEIDENTIFIER
SELECT @chargeOwnerId = Id FROM charges.marketparticipant WHERE marketparticipantid = '8100000000030'
DECLARE @chargeId UNIQUEIDENTIFIER = NEWID()

INSERT INTO [Charges].[Charge] VALUES (@chargeId, 'TestTar001', 3, @chargeOwnerId, 0, 2)
INSERT INTO [Charges].[ChargePeriod] VALUES (NEWID(), @chargeId, 1, 'Period1', 'Tariff with multiple periods', 1, '2021-12-31 23:00:00', '2022-10-31 23:00:00')
INSERT INTO [Charges].[ChargePeriod] VALUES (NEWID(), @chargeId, 1, 'Period2', 'Tariff with multiple periods A', 1, '2022-10-31 23:00:00', '2023-05-31 22:00:00')
INSERT INTO [Charges].[ChargePeriod] VALUES (NEWID(), @chargeId, 1, 'Period3', 'Tariff with multiple periods A', 1, '2023-05-31 22:00:00', '2024-01-31 23:00:00')
INSERT INTO [Charges].[ChargePeriod] VALUES (NEWID(), @chargeId, 1, 'Period4', 'Tariff with multiple periods A', 1, '2024-01-31 23:00:00', '9999-12-31 23:59:59')