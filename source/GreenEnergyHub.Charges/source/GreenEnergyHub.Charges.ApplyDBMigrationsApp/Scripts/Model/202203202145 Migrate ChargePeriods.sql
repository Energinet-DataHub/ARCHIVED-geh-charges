
begin tran

------------------------------------------------------------------------------------------------------------------------
-- Add ReceivedDateTime, ReceivedOrder and IsStop, 
------------------------------------------------------------------------------------------------------------------------

    alter table [Charges].[ChargePeriod]
        add
            [ReceivedDateTime] [datetime2](7) null,
            [ReceivedOrder] [int] not null default (0), -- Default 0
            [IsStop] [bit] not null default (0) -- Default false
    go

------------------------------------------------------------------------------------------------------------------------
-- Migrate ChargePeriods set ReceivedDateTime to some time before StartDateTime 
------------------------------------------------------------------------------------------------------------------------

    update [Charges].[ChargePeriod]
    set ReceivedDateTime = dateadd(month, -1, StartDateTime)
    go

------------------------------------------------------------------------------------------------------------------------
-- Migrate ChargePeriods with EndDateTime = StartDateTime to IsStop
------------------------------------------------------------------------------------------------------------------------
    
    update [Charges].[ChargePeriod]
    set IsStop = 'True'
    where EndDateTime = StartDateTime
    go

------------------------------------------------------------------------------------------------------------------------
-- Set ReceivedDateTime not null
------------------------------------------------------------------------------------------------------------------------

    alter table [Charges].[ChargePeriod]
        alter column [ReceivedDateTime] [datetime2](7) not null
    go

------------------------------------------------------------------------------------------------------------------------
-- Remove EndDateTime
------------------------------------------------------------------------------------------------------------------------
    
    alter table [Charges].[ChargePeriod]
        drop column [EndDateTime]
    go

commit
