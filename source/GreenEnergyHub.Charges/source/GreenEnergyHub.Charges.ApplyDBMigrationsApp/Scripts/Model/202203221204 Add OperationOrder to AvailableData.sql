------------------------------------------------------------------------------------------------------------------------
-- Add OperationOrder to AvailableData
-- AvailableChargeData
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD [OperationOrder] [int] NOT NULL DEFAULT (0)
GO
