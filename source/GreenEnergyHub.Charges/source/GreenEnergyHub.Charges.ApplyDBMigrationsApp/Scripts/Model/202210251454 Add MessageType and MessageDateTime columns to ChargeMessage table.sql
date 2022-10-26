------------------------------------------------------------------------------------------------------------------------
-- Add MessageType and MessageDateTime to ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE Charges.ChargeMessage
ADD [MessageType] [int] CONSTRAINT DF_ChargeMessage_MessageType default 0 NOT NULL,
    [MessageDateTime] [datetime2](7) CONSTRAINT DF_ChargeMessage_MessageDateTime default getdate() NOT NULL
GO

ALTER TABLE Charges.ChargeMessage DROP CONSTRAINT DF_ChargeMessage_MessageType
ALTER TABLE Charges.ChargeMessage DROP CONSTRAINT DF_ChargeMessage_MessageDateTime
GO