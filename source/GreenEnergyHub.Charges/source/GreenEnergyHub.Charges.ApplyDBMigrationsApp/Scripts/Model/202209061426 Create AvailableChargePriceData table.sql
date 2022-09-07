------------------------------------------------------------------------------------------------------------------------
-- Tables
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [MessageHub].[AvailableChargePriceData](
    [Id] [uniqueidentifier] NOT NULL,
    [RecipientId] [nvarchar](35) NOT NULL,
    [RecipientRole] [int] NOT NULL,
    [BusinessReasonCode] [int] NOT NULL,
    [ChargeId] [nvarchar](35) NOT NULL,
    [ChargeOwner] [nvarchar](35) NOT NULL,
    [ChargeType] [int] NOT NULL,
    [StartDateTime] [datetime2](7) NOT NULL,
    [EndDateTime] [datetime2](7) NOT NULL,
    [Resolution] [int] NOT NULL,
    [RequestDateTime] [datetime2](7) NOT NULL,
    [AvailableDataReferenceId] [uniqueidentifier] NOT NULL,
    [SenderId] [nvarchar](35) NOT NULL,
    [SenderRole] [int] NOT NULL,
    [DocumentType] [int] NOT NULL,
    [OperationOrder] [int] NOT NULL,
    [ActorId] [uniqueidentifier] NOT NULL
    CONSTRAINT [PK_AvailableChargePriceData] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
GO

CREATE TABLE [MessageHub].[AvailableChargePriceDataPoints](
    [Id] [uniqueidentifier] NOT NULL,
    [AvailableChargePriceDataId] [uniqueidentifier] NOT NULL,
    [Position] [int] NOT NULL,
    [Price] [decimal](14, 6) NOT NULL,
    CONSTRAINT [PK_AvailableChargePriceDataPoints] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
GO

------------------------------------------------------------------------------------------------------------------------
-- Foreign keys
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [MessageHub].[AvailableChargePriceDataPoints]
    WITH CHECK ADD CONSTRAINT [FK_AvailableChargePriceDataPoints_AvailableChargePriceData] FOREIGN KEY([AvailableChargePriceDataId])
    REFERENCES [MessageHub].[AvailableChargePriceData] ([Id])
GO
