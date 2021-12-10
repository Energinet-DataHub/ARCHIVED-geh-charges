-- Move data from ChargeLinkPeriodDetail to ChargeLink before removing the details table in a subsequent migration
-- Algorithm:
-- - Add column to charge table
-- - Set column value from details table (in order to preserve existing data)
-- - Apply NOT NULL constraint if applicable

-------------------------------------
-- StartDateTime
-------------------------------------
ALTER TABLE [Charges].[ChargeLink]
    ADD StartDateTime DateTime2;
GO

UPDATE [Charges].[ChargeLink]
SET StartDateTime = d.StartDateTime
FROM [Charges].[ChargeLink] cl
    INNER JOIN Charges.ChargeLinkPeriodDetails d ON d.ChargeLinkId = cl.Id
    GO

ALTER TABLE [Charges].[ChargeLink]
ALTER COLUMN StartDateTime DateTime2 NOT NULL;
GO

-------------------------------------
-- EndDateTime
-------------------------------------
ALTER TABLE [Charges].[ChargeLink]
    ADD EndDateTime DateTime2;
GO

UPDATE [Charges].[ChargeLink]
SET EndDateTime = d.EndDateTime
FROM [Charges].[ChargeLink] cl
    INNER JOIN Charges.ChargeLinkPeriodDetails d ON d.ChargeLinkId = cl.Id
    GO

ALTER TABLE [Charges].[ChargeLink]
ALTER COLUMN EndDateTime DateTime2 NOT NULL;
GO

-------------------------------------
-- Factor
-------------------------------------
ALTER TABLE [Charges].[ChargeLink]
    ADD Factor int;
GO

UPDATE [Charges].[ChargeLink]
SET Factor = d.Factor
FROM [Charges].[ChargeLink] cl
    INNER JOIN Charges.ChargeLinkPeriodDetails d ON d.ChargeLinkId = cl.Id
    GO

ALTER TABLE [Charges].[ChargeLink]
ALTER COLUMN Factor int NOT NULL;
GO

-------------------------------------
-- Drop deprecated tables
-------------------------------------
DROP TABLE Charges.ChargeLinkPeriodDetails
DROP TABLE Charges.ChargeLinkOperation
GO
