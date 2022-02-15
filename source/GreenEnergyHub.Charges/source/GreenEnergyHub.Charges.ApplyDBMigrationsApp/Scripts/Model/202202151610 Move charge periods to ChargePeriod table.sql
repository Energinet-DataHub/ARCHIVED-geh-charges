------------------------------------------------------------------------------------------------------------------------
-- Add table ChargePeriod
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [Charges].[ChargePeriod](
    [Id] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
    [TaxIndicator] [bit] NOT NULL,
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
-- Insert periods into ChargePeriod table from Charge table
------------------------------------------------------------------------------------------------------------------------

    INSERT INTO [Charges].[ChargePeriod](
    [TaxIndicator],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
[EndDateTime])
SELECT [TaxIndicator],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
    [EndDateTime]
FROM [Charges].[Charge]

------------------------------------------------------------------------------------------------------------------------
-- Remove period related columns from Charge table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[Charge] DROP COLUMN
    [TaxIndicator],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
    [EndDateTime]
    GO
