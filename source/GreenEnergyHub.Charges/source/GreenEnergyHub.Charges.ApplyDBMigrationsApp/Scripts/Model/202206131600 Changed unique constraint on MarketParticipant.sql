------------------------------------------------------------------------------------------------------------------------
-- Drop constraint on MarketParticipant.MarketParticipantId (GLN / EIC)
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [Charges].[MarketParticipant]
    DROP CONSTRAINT UC_MarketParticipantId
GO

------------------------------------------------------------------------------------------------------------------------
-- Replace with new constraint MarketParticipantId and BusinessProcessRole
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [Charges].[MarketParticipant]
    ADD CONSTRAINT [UC_MarketParticipantId_BusinessProcessRole] UNIQUE (MarketParticipantId, BusinessProcessRole)
GO