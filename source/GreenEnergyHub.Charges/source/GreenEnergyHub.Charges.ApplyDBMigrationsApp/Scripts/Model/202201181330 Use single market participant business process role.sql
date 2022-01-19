ALTER TABLE Charges.MarketParticipant
ALTER COLUMN Roles INT
GO

EXECUTE sp_rename 'Charges.MarketParticipant.Roles', 'BusinessProcessRole', 'COLUMN';
