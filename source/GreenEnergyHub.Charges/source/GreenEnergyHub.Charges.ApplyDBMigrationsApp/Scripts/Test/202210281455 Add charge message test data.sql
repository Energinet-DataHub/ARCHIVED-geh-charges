------------------------------------------------------------------------------------------------------------------------
-- Add charge message data
------------------------------------------------------------------------------------------------------------------------

DECLARE @chargeMessageId UNIQUEIDENTIFIER = 'b263c2c6-9d35-4ef5-9658-4e45f4acaf4c';

INSERT INTO [Charges].[ChargeMessage] VALUES (@chargeMessageId,'TestTariff', 3, '8100000000030', 'SeededMessageId1', '2', '2022-10-28T14:58:00');
