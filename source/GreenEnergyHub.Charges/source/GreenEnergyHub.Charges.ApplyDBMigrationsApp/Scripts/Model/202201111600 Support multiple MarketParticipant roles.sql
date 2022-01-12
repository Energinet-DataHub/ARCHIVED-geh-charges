-- Support market participant having multiple roles
EXECUTE sp_rename 'Charges.MarketParticipant.BusinessProcessRole', 'Roles', 'COLUMN';
GO

ALTER TABLE Charges.MarketParticipant
    ALTER COLUMN  Roles VARCHAR(512);
GO
