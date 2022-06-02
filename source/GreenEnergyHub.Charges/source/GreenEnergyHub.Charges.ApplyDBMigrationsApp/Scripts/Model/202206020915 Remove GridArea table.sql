------------------------------------------------------------------------------------------------------------------------
-- Add OwnerId to GridAreaLink and populate with GridAccessProvider from GridArea
------------------------------------------------------------------------------------------------------------------------
BEGIN TRAN

ALTER TABLE [Charges].[GridAreaLink]
    ADD [OwnerId] [uniqueidentifier] NULL
GO

ALTER TABLE [Charges].[GridAreaLink]
    DROP CONSTRAINT FK_GridAreaLink_GridArea;
GO

ALTER TABLE [Charges].[GridAreaLink]
    ADD CONSTRAINT FK_OwnerId_
    FOREIGN KEY (OwnerId) REFERENCES Persons(PersonID);
GO

UPDATE [Charges].[GridAreaLink]
    SET [Charges].[GridAreaLink].[OwnerId] = [Charges].[GridArea].[GridAccessProviderId]
    FROM [Charges].[GridAreaLink]
    JOIN [Charges].[GridArea] ON [Charges].[GridArea].[Id] = [Charges].[GridAreaLink].[GridAreaId]
GO

ALTER TABLE [Charges].[GridAreaLink]
    ADD [OwnerId] [uniqueidentifier] NOT NULL
GO

------------------------------------------------------------------------------------------------------------------------
-- Drop Charges.GridArea table
------------------------------------------------------------------------------------------------------------------------
DROP TABLE [Charges].[GridArea]
GO

COMMIT TRAN