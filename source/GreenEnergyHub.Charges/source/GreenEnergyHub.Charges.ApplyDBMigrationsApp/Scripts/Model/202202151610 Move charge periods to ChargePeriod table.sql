------------------------------------------------------------------------------------------------------------------------
-- Add table ChargePeriod
------------------------------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS Charges.ChargePeriod
GO

CREATE TABLE [Charges].[ChargePeriod](
    [Id] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
    [ChargeId] [uniqueidentifier] NOT NULL,
    [TransparentInvoicing] [bit] NOT NULL,
    [Description] [nvarchar](2048) NOT NULL,
    [Name] [nvarchar](132) NOT NULL,
    [VatClassification] [int] NOT NULL,
    [StartDateTime] [datetime2](7) NOT NULL,
    [EndDateTime] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_ChargePeriod] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

------------------------------------------------------------------------------------------------------------------------
-- Set ChargeId foreign key
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[ChargePeriod]  WITH CHECK ADD  CONSTRAINT [FK_ChargePeriod_Charge] FOREIGN KEY([ChargeId])
    REFERENCES [Charges].[Charge] ([Id])
    GO

------------------------------------------------------------------------------------------------------------------------
-- Insert periods into ChargePeriod table from Charge table
------------------------------------------------------------------------------------------------------------------------

INSERT INTO [Charges].[ChargePeriod](
    [ChargeId],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
[EndDateTime])
SELECT [Id],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
    [EndDateTime]
FROM [Charges].[Charge]
GO

------------------------------------------------------------------------------------------------------------------------
-- Remove period related columns from Charge table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[Charge] DROP COLUMN
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
    [EndDateTime]
    GO
