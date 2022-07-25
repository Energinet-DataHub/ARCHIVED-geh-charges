------------------------------------------------------------------------------------------------------------------------
-- Add test price points
------------------------------------------------------------------------------------------------------------------------
DECLARE @tariff_id UNIQUEIDENTIFIER = NEWID();
DECLARE @fee_id UNIQUEIDENTIFIER = NEWID();
DECLARE @subscription_id UNIQUEIDENTIFIER = NEWID();

DECLARE @tariff_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestTariff');
DECLARE @fee_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestFee');
DECLARE @subscription_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestSub');

DECLARE @point_time [datetime2] = '2020-02-1 23:00:00';

INSERT INTO [Charges].[ChargePoint] VALUES (@tariff_id, @tariff_charge_id, @point_time, 1.0, 1);
INSERT INTO [Charges].[ChargePoint] VALUES (@fee_id, @fee_charge_id, @point_time, 1.0, 1);
INSERT INTO [Charges].[ChargePoint] VALUES (@subscription_id, @subscription_charge_id, @point_time, 1.0, 1);