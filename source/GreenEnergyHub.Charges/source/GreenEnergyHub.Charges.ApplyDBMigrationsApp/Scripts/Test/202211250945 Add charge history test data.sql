------------------------------------------------------------------------------------------------------------------------
-- Add charge history test data
------------------------------------------------------------------------------------------------------------------------

DECLARE @chargeOwner nvarchar(255) = '8100000000030';
DECLARE @endDefaultDateTime nvarchar(20) = '9999-12-31 23:59:59';
DECLARE @tariffA nvarchar(35) = 'TariffA'

INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TestTar001', 3, @chargeOwner, 'Name', 'Description', 2, 0, 0, 1, '2021-12-31 23:00:00', '2022-12-31 23:00:00', '2021-10-31 23:00:00')                                        
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A0', 'Description A0', 2, 0, 0, 1, '2022-01-31 23:00:00', @endDefaultDateTime, '2022-01-01 07:02:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A1', 'Description A1', 2, 0, 0, 1, '2022-01-31 23:00:00', @endDefaultDateTime, '2022-01-01 07:15:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name B', 'Description B', 2, 0, 0, 1, '2022-03-31 22:00:00', @endDefaultDateTime, '2022-01-03 21:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A2', 'Description A2', 2, 0, 0, 1, '2022-01-31 23:00:00', '2022-03-31 22:00:00', '2022-01-04 08:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name future', 'Description future', 2, 0, 0, 1, '2023-12-31 23:00:00', '2022-12-31 23:00:00', '2022-01-05 04:01:00')
