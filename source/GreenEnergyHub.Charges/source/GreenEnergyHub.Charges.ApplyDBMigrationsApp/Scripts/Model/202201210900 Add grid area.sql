CREATE TABLE [Charges].[GridArea](
    [Id] [uniqueidentifier] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [GridAccessProviderId] [uniqueidentifier] NULL,
    CONSTRAINT [PK_GridArea] PRIMARY KEY NONCLUSTERED
    (
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [Charges].[GridArea]  WITH CHECK ADD  CONSTRAINT [FK_GridArea_MarketParticipant] FOREIGN KEY([GridAccessProviderId])
    REFERENCES [Charges].[MarketParticipant] ([Id])
GO

ALTER TABLE [Charges].[GridArea] CHECK CONSTRAINT [FK_GridArea_MarketParticipant]
    GO
