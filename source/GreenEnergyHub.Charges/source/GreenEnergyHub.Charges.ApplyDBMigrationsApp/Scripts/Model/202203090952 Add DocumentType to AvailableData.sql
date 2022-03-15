------------------------------------------------------------------------------------------------------------------------
-- Add DocumentType to AvailableData 
-- AvailableChargeData, AvailableChargeReceiptData, AvailableChargeLinksData, AvailableChargeLinksReceiptData
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD [DocumentType] [int] NOT NULL DEFAULT (0) -- Default Unknown
GO

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ADD [DocumentType] [int] NOT NULL DEFAULT (0) -- Default Unknown
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ADD [DocumentType] [int] NOT NULL DEFAULT (0) -- Default Unknown
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
    ADD [DocumentType] [int] NOT NULL DEFAULT (0) -- Default Unknown
GO