------------------------------------------------------------------------------------------------------------------------
-- Ensure data consistency before migration
------------------------------------------------------------------------------------------------------------------------

begin tran
  
    select
           Id,
           row_number() over(partition by SenderProvidedChargeId, [Type], OwnerId order by SenderProvidedChargeId, [Type], OwnerId) as rownumber
    into Duplicates
    from Charges.Charge
    
    delete cl
    from Charges.ChargeLink as cl
        inner join Duplicates as dpl
        on dpl.Id = cl.ChargeId
    where dpl.rownumber > 1

    delete cp
    from Charges.ChargePoint as cp
        inner join Duplicates as dpl
        on dpl.Id = cp.ChargeId
    where dpl.rownumber > 1

    delete c
    from Charges.Charge as c
        inner join Duplicates as dpl
        on dpl.Id = c.Id
    where dpl.rownumber > 1

commit

------------------------------------------------------------------------------------------------------------------------
-- Add table ChargePeriod
------------------------------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS Charges.ChargePeriod
GO

CREATE TABLE [Charges].[ChargePeriod](
    [Id] [uniqueidentifier] NOT NULL,
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
    [Id],
    [ChargeId],
    [TransparentInvoicing],
    [Description],
    [Name],
    [VatClassification],
    [StartDateTime],
    [EndDateTime])
SELECT 
    NEWID(),
    [Id],
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
