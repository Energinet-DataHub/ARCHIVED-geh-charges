BEGIN TRANSACTION

ALTER TABLE [MessageHub].[AvailableChargeData]
ALTER COLUMN [TransparentInvoicing] [int] NOT NULL

ALTER TABLE [MessageHub].[AvailableChargeData]
ALTER COLUMN [TaxIndicator] [int] NOT NULL

GO

UPDATE [MessageHub].[AvailableChargeData]
SET [TaxIndicator] = 2
WHERE [TaxIndicator] = 1

GO

UPDATE [MessageHub].[AvailableChargeData]
SET [TaxIndicator] = 1
WHERE [TaxIndicator] = 0

GO

UPDATE [MessageHub].[AvailableChargeData]
SET [TransparentInvoicing] = 2
WHERE [TransparentInvoicing] = 1

GO

UPDATE [MessageHub].[AvailableChargeData]
SET [TransparentInvoicing] = 1
WHERE [TransparentInvoicing] = 0

GO

COMMIT