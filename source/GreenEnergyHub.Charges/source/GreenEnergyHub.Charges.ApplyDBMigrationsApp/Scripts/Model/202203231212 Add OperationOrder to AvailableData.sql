------------------------------------------------------------------------------------------------------------------------
-- Add OperationOrder to AvailableData
-- AvailableChargeData, AvailableChargeReceiptData, AvailableChargeLinksData, AvailableChargeLinksReceiptData
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD [OperationOrder] [int] NOT NULL DEFAULT (0)
GO

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ADD [OperationOrder] [int] NOT NULL DEFAULT (0)
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ADD [OperationOrder] [int] NOT NULL DEFAULT (0)
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
    ADD [OperationOrder] [int] NOT NULL DEFAULT (0)
GO