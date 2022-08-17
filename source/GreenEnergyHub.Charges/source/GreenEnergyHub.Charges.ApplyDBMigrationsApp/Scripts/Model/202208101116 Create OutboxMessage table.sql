------------------------------------------------------------------------------------------------------------------------
-- Tables
------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [Charges].[OutboxMessage](
    [Id] [uniqueidentifier] NOT NULL,
    [Type] [nvarchar](255) NOT NULL,
    [Data] [nvarchar](max) NOT NULL,
    [CorrelationId] [nvarchar](255) NOT NULL,
    [CreationDate] [datetime2](7) NOT NULL,
    [ProcessedDate] [datetime2](7) NULL,
    CONSTRAINT [PK_OutboxMessage] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO