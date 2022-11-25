------------------------------------------------------------------------------------------------------------------------
-- Add charge history test data
------------------------------------------------------------------------------------------------------------------------

INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TestTar001', 3, '8100000000030', 'Name', 'Description', 2, 0, 0, 1, '2021-12-31 23:00:00', '2022-12-31 23:00:00', '2021-10-31 23:00:00')
                                           
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TariffA', 3, '8100000000030', 'Name A0', 'Description', 2, 0, 0, 1, '2022-01-31 23:00:00', '9999-12-31 23:59:59', '2022-01-01 07:02:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TariffA', 3, '8100000000030', 'Name A1', 'Description', 2, 0, 0, 1, '2022-01-31 23:00:00', '9999-12-31 23:59:59', '2022-01-01 07:15:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TariffA', 3, '8100000000030', 'Name B', 'Description', 2, 0, 0, 1, '2022-03-31 22:00:00', '9999-12-31 23:59:59', '2022-01-03 21:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TariffA', 3, '8100000000030', 'Name A2', 'Description', 2, 0, 0, 1, '2022-01-31 23:00:00', '2022-03-31 22:00:00', '2022-01-04 08:00:00')
INSERT INTO [ChargesQuery].[ChargeHistory] VALUES (NEWID(), 'TariffA', 3, '8100000000030', 'Name future', 'Description', 2, 0, 0, 1, '2023-12-31 23:00:00', '2022-12-31 23:00:00', '2022-01-05 04:01:00')
