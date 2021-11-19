EXECUTE sp_rename 'MessageHub.AvailableChargeData.RequestTime', 'RequestDateTime', 'COLUMN';
GO

EXECUTE sp_rename 'MessageHub.AvailableChargeLinksData.RequestTime', 'RequestDateTime', 'COLUMN';
GO

EXECUTE sp_rename 'MessageHub.AvailableChargeLinkReceiptData.RequestTime', 'RequestDateTime', 'COLUMN';
GO