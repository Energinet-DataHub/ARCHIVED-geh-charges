EXEC sp_rename 'Charges.MarketParticipant.Active', 'IsActive', 'COLUMN';
GO

EXEC sp_rename 'Charges.MarketParticipant.Role', 'BusinessProcessRole', 'COLUMN';
GO

ALTER TABLE [Charges].[MarketParticipant]
DROP COLUMN Name
GO
