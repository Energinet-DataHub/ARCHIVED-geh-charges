
begin tran
 
    ------------------------------------------------------------------------------------------------------------------------
    -- Remove DataAvailable tables' foreign key.
    -- This is needed for successfully force updating market participants data in Charges through integration events
    -- from the Market Participant domain)
    ------------------------------------------------------------------------------------------------------------------------

    alter table MessageHub.AvailableChargeData
    drop constraint FK_AvailableChargeData_ActorId
        go
    
    alter table MessageHub.AvailableChargeReceiptData
    drop constraint FK_AvailableChargeReceiptData_ActorId
        go
    
    alter table MessageHub.AvailableChargeLinksData
    drop constraint FK_AvailableChargeLinksData_ActorId
        go
    
    alter table MessageHub.AvailableChargeLinksReceiptData
    drop constraint FK_AvailableChargeLinksReceiptData_ActorId
    go
   
commit