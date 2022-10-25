------------------------------------------------------------------------------------------------------------------------
-- Add MessageType and MessageDate to ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE Charges.ChargeMessage
ADD [MessageType] [int] not null default 0,
    [MessageDateTime] [datetime2](7) NOT NULL default getdate()
GO
