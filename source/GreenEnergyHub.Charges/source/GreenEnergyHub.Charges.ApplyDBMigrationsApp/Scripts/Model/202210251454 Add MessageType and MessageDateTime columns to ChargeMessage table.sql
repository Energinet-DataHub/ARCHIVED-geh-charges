------------------------------------------------------------------------------------------------------------------------
-- Add MessageType and MessageDate to ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE Charges.ChargeMessage
ADD [MessageType] [int] not null default 0,
    [MessageDateTime] [datetime2](7) NOT NULL default getdate()
GO

ALTER TABLE Charges.ChargeMessage ALTER COLUMN [MessageType] [int] not null
ALTER TABLE Charges.ChargeMessage ALTER COLUMN [MessageDateTime] [datetime2](7) NOT NULL
GO