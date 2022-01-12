ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD [SenderId] NVARCHAR(35) NOT NULL DEFAULT('5790001330552');
GO

ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD [SenderRole] INT NOT NULL DEFAULT(7);
GO

ALTER TABLE [MessageHub].[AvailableChargeLinkReceiptData]
    ADD [SenderId] NVARCHAR(35) NOT NULL DEFAULT('5790001330552');
GO

ALTER TABLE [MessageHub].[AvailableChargeLinkReceiptData]
    ADD [SenderRole] INT NOT NULL DEFAULT(7);
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ADD [SenderId] NVARCHAR(35) NOT NULL DEFAULT('5790001330552');
GO

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ADD [SenderRole] INT NOT NULL DEFAULT(7);
GO

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ADD [SenderId] NVARCHAR(35) NOT NULL DEFAULT('5790001330552');
GO

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ADD [SenderRole] INT NOT NULL DEFAULT(7);
GO