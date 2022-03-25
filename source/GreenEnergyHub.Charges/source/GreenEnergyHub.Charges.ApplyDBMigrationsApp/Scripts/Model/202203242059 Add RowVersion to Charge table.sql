------------------------------------------------------------------------------------------------------------------------
-- Adds 'rowversion' to the Charge table, needed for optimistic concurrency handling
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [Charges].[Charge]
    ADD [Timestamp] rowversion
GO