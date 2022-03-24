
begin tran

------------------------------------------------------------------------------------------------------------------------
-- Add columns from ChargePeriod on Charge table 
------------------------------------------------------------------------------------------------------------------------

    alter table [Charges].[Charge]
    add
    [TransparentInvoicing] bit null,
    [Description] nvarchar(2048) null,
    [Name] nvarchar(132) null,
    [VatClassification] int null,
    [StartDateTime] [datetime2](7) null,
    [ReceivedDateTime] [datetime2](7) null,
    [ReceivedOrder] [int] not null default (0), -- Default 0
    [IsStop] [bit] not null default (0) -- Default false
    go

------------------------------------------------------------------------------------------------------------------------
-- Update Charge with data from first ChargePeriod
------------------------------------------------------------------------------------------------------------------------

    update chg
    set chg.TransparentInvoicing = chp.TransparentInvoicing,
        chg.Description = chp.Description,
        chg.Name = chp.Name,
        chg.VatClassification = chp.VatClassification,
        chg.StartDateTime = chp.StartDateTime,
        chg.ReceivedDateTime = chp.ReceivedDateTime,
        chg.ReceivedOrder = chp.ReceivedOrder,
        chg.IsStop = chp.IsStop
        from [Charges].[Charge] as chg
            inner join [Charges].[ChargePeriod] as chp
    on chp.chargeId = chg.Id
    where
        chp.Id = (
            select top 1 chp2.Id
            from [Charges].[ChargePeriod] as chp2
            where ChargeID = chg.ID
            order by StartDateTime asc
        )
    go

------------------------------------------------------------------------------------------------------------------------
-- Delete first ChargePeriod
------------------------------------------------------------------------------------------------------------------------

    delete chp
    from [Charges].[Charge] as chg
        inner join [Charges].[ChargePeriod] as chp
        on chp.chargeId = chg.Id
    where
        chp.Id = (
            select top 1 chp2.Id
            from [Charges].[ChargePeriod] as chp2
            where ChargeID = chg.ID
            order by StartDateTime asc
        )
    go

------------------------------------------------------------------------------------------------------------------------
-- Denormalize remaining ChargePeriods to Charges
------------------------------------------------------------------------------------------------------------------------

    insert into [Charges].[Charge] (
        Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, Description, 
        [Name], VatClassification, StartDateTime, ReceivedDateTime, ReceivedOrder, IsStop)
    select chp.id, chg.SenderProvidedChargeId, chg.[Type], chg.OwnerId, chg.TaxIndicator, chg.Resolution,
           chg.TransparentInvoicing, chg.Description, chg.Name, chg.VatClassification, chg.StartDateTime,
           chg.ReceivedDateTime, chg.ReceivedOrder, chg.IsStop
    from [Charges].[Charge] as chg
        inner join [Charges].[ChargePeriod] as chp
    on chp.chargeId = chg.Id
    go

------------------------------------------------------------------------------------------------------------------------
-- Set new Charge columns not null
------------------------------------------------------------------------------------------------------------------------

    alter table [Charges].[Charge] alter column [ReceivedDateTime] [datetime2](7) not null
    alter table [Charges].[Charge] alter column [TransparentInvoicing] bit not null
    alter table [Charges].[Charge] alter column [Description] nvarchar(2048) not null
    alter table [Charges].[Charge] alter column [Name] nvarchar(132) not null
    alter table [Charges].[Charge] alter column [VatClassification] int not null
    alter table [Charges].[Charge] alter column [StartDateTime] [datetime2](7) not null
    alter table [Charges].[Charge] alter column [ReceivedDateTime] [datetime2](7) not null
    alter table [Charges].[Charge] alter column [ReceivedOrder] [int] not null
    alter table [Charges].[Charge] alter column [IsStop] [bit] not null
    go

------------------------------------------------------------------------------------------------------------------------
-- Drop ChargePeriod table
------------------------------------------------------------------------------------------------------------------------

    drop table [Charges].[ChargePeriod]
    go

commit