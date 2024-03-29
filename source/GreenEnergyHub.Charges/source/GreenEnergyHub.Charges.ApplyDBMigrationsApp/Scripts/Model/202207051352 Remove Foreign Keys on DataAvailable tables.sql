﻿
begin tran
 
    ------------------------------------------------------------------------------------------------------------------------
    -- Remove DataAvailable tables' foreign key.
    -- This is needed for successfully updating market participants data into the Charges domain by force-pushing
    -- integration events from the Market Participant domain
    ------------------------------------------------------------------------------------------------------------------------

    alter table MessageHub.AvailableChargeData
    drop constraint FK_AvailableChargeData_MarketParticipantActorId
        go
    
    alter table MessageHub.AvailableChargeReceiptData
    drop constraint FK_AvailableChargeReceiptData_MarketParticipantActorId
        go
    
    alter table MessageHub.AvailableChargeLinksData
    drop constraint FK_AvailableChargeLinksData_MarketParticipantActorId
        go
    
    alter table MessageHub.AvailableChargeLinksReceiptData
    drop constraint FK_AvailableChargeLinksReceiptData_MarketParticipantActorId
        go
   
commit