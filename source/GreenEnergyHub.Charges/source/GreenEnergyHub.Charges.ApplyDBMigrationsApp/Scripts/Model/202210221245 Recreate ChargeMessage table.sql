------------------------------------------------------------------------------------------------------------------------
-- Drop ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS Charges.ChargeMessage

------------------------------------------------------------------------------------------------------------------------
-- Create ChargeMessage table
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [Charges].[ChargeMessage](
  [Id] [uniqueidentifier] NOT NULL,
  [SenderProvidedChargeId] [nvarchar](35) NOT NULL,
  [Type] [int] NOT NULL,
  [MarketParticipantId] [nvarchar](35) NOT NULL,
  [MessageId] [nvarchar](255) NOT NULL,
  CONSTRAINT [PK_ChargeMessage] PRIMARY KEY NONCLUSTERED
      (
       [Id] ASC
          )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO