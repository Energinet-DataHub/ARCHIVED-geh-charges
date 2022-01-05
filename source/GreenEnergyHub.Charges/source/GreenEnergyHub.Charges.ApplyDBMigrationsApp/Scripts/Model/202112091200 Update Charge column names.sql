-- Align column names with domain model

EXEC sp_rename 'Charges.Charge.MarketParticipantId', 'OwnerId', 'COLUMN';
GO

EXEC sp_rename 'Charges.Charge.ChargeType', 'Type', 'COLUMN';
GO
