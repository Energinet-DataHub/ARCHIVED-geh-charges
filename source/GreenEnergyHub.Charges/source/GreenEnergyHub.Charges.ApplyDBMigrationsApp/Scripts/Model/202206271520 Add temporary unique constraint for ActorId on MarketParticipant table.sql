------------------------------------------------------------------------------------------------------------------------
-- Add unique constraint on ActorId to Charges.MarketParticipant.
-- This is considered a temporary guard, which will be beneficial to have while transitioning to integration events
-- published by the MarketParticipant domain. Especially, when invoking 'force send integration events'.  
-- Temporary until Charges allow multiple business process roles per ActorId. 
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[MarketParticipant]
    ADD CONSTRAINT [UC_ActorId] UNIQUE (ActorId);
GO