------------------------------------------------------------------------------------------------------------------------
-- Create ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [Charges].[ChargeMessage](
    [Id] [uniqueidentifier] NOT NULL,
    [ChargeId] [uniqueidentifier] NOT NULL,
    [MessageId] [nvarchar](255) NOT NULL,
    CONSTRAINT [PK_ChargeMessage] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

------------------------------------------------------------------------------------------------------------------------
-- Foreign keys
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[ChargeMessage]
    WITH CHECK ADD CONSTRAINT [FK_ChargeMessage_Charge] FOREIGN KEY([ChargeId])
        REFERENCES [Charges].[Charge] ([Id])
GO