------------------------------------------------------------------------------------------------------------------------
-- Add charge history test data
------------------------------------------------------------------------------------------------------------------------

DECLARE @chargeOwner NVARCHAR(255) = '8100000000030';
DECLARE @tariffA NVARCHAR(35) = 'TariffA';
DECLARE @endDefaultDateTime NVARCHAR(20) = '9999-12-31 23:59:59';

INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'HistTar001', 3, @chargeOwner, 'HistTar001 Name', 'HistTar001 Description', 2, 0, 0, 1, '2021-12-31 23:00:00', '2022-12-31 23:00:00', '2021-10-31 23:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A0', 'Description A0', 2, 0, 0, 1, '2022-01-31 23:00:00', @endDefaultDateTime, '2022-01-01 07:02:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A1', 'Description A1', 2, 0, 0, 1, '2022-01-31 23:00:00', @endDefaultDateTime, '2022-01-01 07:15:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name B', 'Description B', 2, 0, 0, 1, '2022-03-31 22:00:00', @endDefaultDateTime, '2022-01-03 21:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name A2', 'Description A2', 2, 0, 0, 1, '2022-01-31 23:00:00', @endDefaultDateTime, '2022-01-04 20:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), @tariffA, 3, @chargeOwner, 'Name future', 'Description future', 2, 0, 0, 1, '2023-12-31 23:00:00', @endDefaultDateTime, '2022-01-05 04:01:00')