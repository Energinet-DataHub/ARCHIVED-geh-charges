-- Move data from ChargePeriodDetail to Charge before removing the details table in a subsequent migration
-- Algorithm:
-- - Add column to charge table
-- - Set column value from details table (in order to preserve existing data)
-- - Apply NOT NULL constraint if applicable

-------------------------------------
-- Description
-------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [Description] nvarchar(2048);
GO

UPDATE [Charges].[Charge]
SET Description = d.Description
FROM [Charges].[Charge] c
INNER JOIN Charges.ChargePeriodDetails d ON d.ChargeId = c.Id   
GO

ALTER TABLE [Charges].[Charge]
ALTER COLUMN Description nvarchar(2048) NOT NULL;
GO

-------------------------------------
-- Name
-------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [Name] nvarchar(132);
GO

UPDATE [Charges].[Charge]
SET Name = d.Name
FROM [Charges].[Charge] c
    INNER JOIN Charges.ChargePeriodDetails d ON d.ChargeId = c.Id
    GO

ALTER TABLE [Charges].[Charge]
ALTER COLUMN Name nvarchar(132) NOT NULL;
GO

-------------------------------------
-- VatClassification
-------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [VatClassification] int;
GO

UPDATE [Charges].[Charge]
SET VatClassification = d.VatClassification
FROM [Charges].[Charge] c
    INNER JOIN Charges.ChargePeriodDetails d ON d.ChargeId = c.Id
    GO

ALTER TABLE [Charges].[Charge]
ALTER COLUMN VatClassification int NOT NULL;
GO

-------------------------------------
-- StartDateTime
-------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [StartDateTime] DateTime2;
GO

UPDATE [Charges].[Charge]
SET StartDateTime = d.StartDateTime
FROM [Charges].[Charge] c
    INNER JOIN Charges.ChargePeriodDetails d ON d.ChargeId = c.Id
    GO

ALTER TABLE [Charges].[Charge]
ALTER COLUMN StartDateTime DateTime2 NOT NULL;
GO

-------------------------------------
-- EndDateTime (nullable)
-------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [EndDateTime] DateTime2;
GO

UPDATE [Charges].[Charge]
SET EndDateTime = d.EndDateTime
FROM [Charges].[Charge] c
    INNER JOIN Charges.ChargePeriodDetails d ON d.ChargeId = c.Id
    GO
