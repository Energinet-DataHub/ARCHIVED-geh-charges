------------------------------------------------------------------------------------------------------------------------
-- Add OwnerId to GridAreaLink and populate with GridAccessProvider from GridArea
------------------------------------------------------------------------------------------------------------------------
ALTER TABLE [Charges].[GridAreaLink]
    ADD [OwnerId] [uniqueidentifier] NULL
GO

ALTER TABLE [Charges].[GridAreaLink]
DROP CONSTRAINT FK_GridAreaLink_GridArea;
GO

UPDATE [Charges].[GridAreaLink]
SET [Charges].[GridAreaLink].[OwnerId] = [Charges].[GridArea].[GridAccessProviderId]
FROM [Charges].[GridAreaLink]
    JOIN [Charges].[GridArea] ON [Charges].[GridArea].[Id] = [Charges].[GridAreaLink].[GridAreaId]
GO

ALTER TABLE [Charges].[GridAreaLink] WITH CHECK
    ADD CONSTRAINT [FK_GridAreaLink_MarketParticipant]
    FOREIGN KEY ([OwnerId]) REFERENCES [Charges].[MarketParticipant] (Id);
GO

------------------------------------------------------------------------------------------------------------------------
-- Drop Charges.GridArea table
------------------------------------------------------------------------------------------------------------------------
DROP TABLE [Charges].[GridArea]
GO