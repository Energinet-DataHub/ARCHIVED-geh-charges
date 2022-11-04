------------------------------------------------------------------------------------------------------------------------
-- Add test price points
------------------------------------------------------------------------------------------------------------------------
DECLARE @tariff_id UNIQUEIDENTIFIER = NEWID();
DECLARE @fee_id UNIQUEIDENTIFIER = NEWID();
DECLARE @subscription_id UNIQUEIDENTIFIER = NEWID();

DECLARE @tariff_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestTariff');
DECLARE @fee_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestFee');
DECLARE @subscription_charge_id UNIQUEIDENTIFIER = (SELECT TOP(1) [Id] from [Charges].[Charge] where [SenderProvidedChargeId] = 'TestSub');

-- Creating 24 ChargePoints for TestTariff, it has a PT1H resolution
DECLARE @point_time [datetime2] = '2022-10-29 22:00:00';
DECLARE @minute_step int = 60
DECLARE @numPoints int = 25
DECLARE @pointCounter int = 1.0;

WHILE @pointCounter <= @numPoints
    BEGIN
        INSERT INTO [Charges].[ChargePoint] VALUES (NEWID(), @tariff_charge_id, @point_time, 1.0 * @pointCounter, 1);
        SET @point_time = DATEADD(minute, @minute_step, @point_time);
        SET @pointCounter = @pointCounter + 1;
    END

INSERT INTO [Charges].[ChargePoint] VALUES (@fee_id, @fee_charge_id, @point_time, 1.0, 1);
INSERT INTO [Charges].[ChargePoint] VALUES (@subscription_id, @subscription_charge_id, @point_time, 1.0, 1);

DECLARE @charge_with_enddate_id UNIQUEIDENTIFIER = NEWID();
DECLARE @chargeOwnerId UNIQUEIDENTIFIER
SELECT @chargeOwnerId = Id FROM charges.marketparticipant WHERE marketparticipantid = '8100000000030'
INSERT INTO [Charges].[Charge] VALUES (@charge_with_enddate_id, 'chg-end', 3, @chargeOwnerId, 0, 2);
INSERT INTO [Charges].[ChargePeriod] VALUES (NEWID(), @charge_with_enddate_id, 0, 'charge with an enddate desc', 'charge with an enddate', 2,  '2020-12-31T23:00:00',  '2021-12-31T23:00:00');