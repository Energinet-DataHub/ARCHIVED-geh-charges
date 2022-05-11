IF COL_LENGTH('Charges.ChargePoint', 'Position') IS NOT NULL
BEGIN
    ALTER TABLE [Charges].[ChargePoint]
    DROP COLUMN Position
END
GO
