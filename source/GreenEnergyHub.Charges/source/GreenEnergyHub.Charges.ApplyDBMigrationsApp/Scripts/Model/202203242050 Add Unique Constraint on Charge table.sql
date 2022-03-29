------------------------------------------------------------------------------------------------------------------------
-- Add unique constraint on Charges.Charge to reject concurrent inserts for the same unique charge,
-- identified by SenderProvidedChargeId, Type and OwnerId
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD CONSTRAINT [UC_SenderProvidedChargeId_Type_OwnerId] UNIQUE (SenderProvidedChargeId, Type, OwnerId);
GO