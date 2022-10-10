------------------------------------------------------------------------------------------------------------------------
-- Add Name to Market Participant table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE Charges.MarketParticipant
ADD Name nvarchar(255) not null default 'Pending charge owner name'
GO