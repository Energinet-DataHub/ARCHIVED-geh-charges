EXECUTE SP_RENAME @objname = 'Charges.MarketParticipant.IsActive', @newname = 'Status', @objtype = 'COLUMN'

ALTER TABLE Charges.MarketParticipant ALTER COLUMN Status INT

GO

UPDATE Charges.MarketParticipant 
    SET Status = 2 
    WHERE Status = 1;
UPDATE Charges.MarketParticipant 
    SET Status = 3 
    WHERE Status = 0;