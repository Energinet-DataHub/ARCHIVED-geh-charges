------------------------------------------------------------------------------------------------------------------------
-- Increase OriginalOperationId length in MessageHub.AvailableChargeLinksReceiptData and AvailableChargeReceiptData
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
ALTER COLUMN [OriginalOperationId] [nvarchar](36) NOT NULL
GO

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
ALTER COLUMN [OriginalOperationId] [nvarchar](36) NOT NULL
GO