------------------------------------------------------------------------------------------------------------------------
-- Add ActorId to AvailableData
-- AvailableChargeData, AvailableChargeReceiptData, AvailableChargeLinksData, AvailableChargeLinksReceiptData
------------------------------------------------------------------------------------------------------------------------
BEGIN TRANSACTION

-- Add foreign key field to table
ALTER TABLE [MessageHub].[AvailableChargeData]
    ADD ActorId [uniqueidentifier]
GO

-- Add foreign key constraint
ALTER TABLE [MessageHub].[AvailableChargeData]
    WITH CHECK ADD  CONSTRAINT [FK_AvailableChargeData_ActorId] FOREIGN KEY([ActorId])
    REFERENCES [Charges].[MarketParticipant] ([Id])
GO

-- Populate field, if any entries in current available table
UPDATE [MessageHub].[AvailableChargeData]
    SET ActorId = COALESCE((SELECT Id FROM [Charges].[MarketParticipant] MP
    WHERE MP.MarketParticipantId = RecipientId AND MP.BusinessProcessRole = RecipientRole), (SELECT TOP 1 Id FROM [Charges].[MarketParticipant] WHERE BusinessProcessRole = 3))
GO

-- Change field to be NOT NULL
ALTER TABLE [MessageHub].[AvailableChargeData]
    ALTER COLUMN ActorId uniqueidentifier NOT NULL;
GO

-- Add foreign key field to table
ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ADD ActorId [uniqueidentifier]
GO

-- Add foreign key constraint
ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    WITH CHECK ADD  CONSTRAINT [FK_AvailableChargeReceiptData_ActorId] FOREIGN KEY([ActorId])
    REFERENCES [Charges].[MarketParticipant] ([Id])
GO

-- Populate field, if any entries in current available table
UPDATE [MessageHub].[AvailableChargeReceiptData]
    SET ActorId = COALESCE((SELECT Id FROM [Charges].[MarketParticipant] MP
    WHERE MP.MarketParticipantId = RecipientId AND MP.BusinessProcessRole = RecipientRole), (SELECT TOP 1 Id FROM [Charges].[MarketParticipant] WHERE BusinessProcessRole = 3))
GO

-- Change field to be NOT NULL
ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
    ALTER COLUMN ActorId uniqueidentifier NOT NULL;
GO

-- Add foreign key field to table
ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ADD ActorId [uniqueidentifier]
GO

-- Add foreign key constraint
ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    WITH CHECK ADD  CONSTRAINT [FK_AvailableChargeLinksData_ActorId] FOREIGN KEY([ActorId])
    REFERENCES [Charges].[MarketParticipant] ([Id])
GO

-- Populate field, if any entries in current available table
UPDATE [MessageHub].[AvailableChargeLinksData]
    SET ActorId = COALESCE((SELECT Id FROM [Charges].[MarketParticipant] MP
    WHERE MP.MarketParticipantId = RecipientId AND MP.BusinessProcessRole = RecipientRole), (SELECT TOP 1 Id FROM [Charges].[MarketParticipant] WHERE BusinessProcessRole = 3))
GO

-- Change field to be NOT NULL
ALTER TABLE [MessageHub].[AvailableChargeLinksData]
    ALTER COLUMN ActorId uniqueidentifier NOT NULL;
GO

-- Add foreign key field to table
ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
    ADD ActorId [uniqueidentifier]
GO

-- Add foreign key constraint
ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
    WITH CHECK ADD  CONSTRAINT [FK_AvailableChargeLinksReceiptData_ActorId] FOREIGN KEY([ActorId])
    REFERENCES [Charges].[MarketParticipant] ([Id])
GO

-- Populate field, if any entries in current available table
UPDATE [MessageHub].[AvailableChargeLinksReceiptData]
    SET ActorId = COALESCE((SELECT Id FROM [Charges].[MarketParticipant] MP
    WHERE MP.MarketParticipantId = RecipientId AND MP.BusinessProcessRole = RecipientRole), (SELECT TOP 1 Id FROM [Charges].[MarketParticipant] WHERE BusinessProcessRole = 3))
GO

-- Change field to be NOT NULL
ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
    ALTER COLUMN ActorId uniqueidentifier NOT NULL;
GO

COMMIT TRANSACTION