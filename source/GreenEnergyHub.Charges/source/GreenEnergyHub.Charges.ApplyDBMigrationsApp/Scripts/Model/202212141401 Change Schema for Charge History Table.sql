------------------------------------------------------------------------------------------------------------------------
-- Remove ChargesQuery Schema and ChargeHistory table within
------------------------------------------------------------------------------------------------------------------------
IF EXISTS (
        SELECT  *
        FROM sys.schemas
        WHERE name = N'ChargesQuery')
    BEGIN
        DROP TABLE [ChargesQuery].[ChargeHistory]
        DROP SCHEMA [ChargesQuery]
    END
GO

------------------------------------------------------------------------------------------------------------------------
-- Recreate ChargeHistory table in Charges Schema
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [Charges].[ChargeHistory](
                                          [Id] [uniqueidentifier] NOT NULL,
                                          [SenderProvidedChargeId] [nvarchar](35) NOT NULL,
                                          [Type] [int] NOT NULL,
                                          [Owner] [nvarchar](35) NOT NULL,
                                          [Name] [nvarchar](132) NOT NULL,
                                          [Description] [nvarchar](2048) NOT NULL,
                                          [Resolution] [int] NOT NULL,
                                          [TaxIndicator] [bit] NOT NULL,
                                          [TransparentInvoicing] [bit] NOT NULL,
                                          [VatClassification] [int] NOT NULL,
                                          [StartDateTime] [datetime2](7) NOT NULL,
                                          [EndDateTime] [datetime2](7) NOT NULL,
                                          [AcceptedDateTime] [datetime2](7) NOT NULL,
                                          CONSTRAINT [PK_ChargeHistory] PRIMARY KEY NONCLUSTERED
                                              (
                                               [Id] ASC
                                                  )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

------------------------------------------------------------------------------------------------------------------------
-- Add index
------------------------------------------------------------------------------------------------------------------------

CREATE NONCLUSTERED INDEX [I1_SenderProvidedChargeId_Type_Owner] ON [Charges].[ChargeHistory]
    (
     [SenderProvidedChargeId] ASC,
     [Type] ASC,
     [Owner] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO