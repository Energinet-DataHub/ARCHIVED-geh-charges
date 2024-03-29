﻿
begin tran

    ------------------------------------------------------------------------------------------------------------------------
    -- Rename ActorId to DeprecatedActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    exec sp_rename 'MessageHub.AvailableChargeData.ActorId', 'DeprecatedActorId', 'COLUMN';
    exec sp_rename 'MessageHub.AvailableChargeReceiptData.ActorId', 'DeprecatedActorId', 'COLUMN';
    exec sp_rename 'MessageHub.AvailableChargeLinksData.ActorId', 'DeprecatedActorId', 'COLUMN';
    exec sp_rename 'MessageHub.AvailableChargeLinksReceiptData.ActorId', 'DeprecatedActorId', 'COLUMN';
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Add ActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    alter table MessageHub.AvailableChargeData
        add ActorId uniqueidentifier null
    go
    
    alter table MessageHub.AvailableChargeReceiptData
        add ActorId uniqueidentifier null
    go
    
    alter table MessageHub.AvailableChargeLinksData
        add ActorId uniqueidentifier null
    go
    
    alter table MessageHub.AvailableChargeLinksReceiptData
        add ActorId uniqueidentifier null
    go
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Migrate Id to ActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    update acd
    set acd.ActorId = mp.ActorId
    from MessageHub.AvailableChargeData as acd
         inner join Charges.MarketParticipant as mp
            on acd.DeprecatedActorId = mp.Id
    
    update acrd
    set acrd.ActorId = mp.ActorId
    from MessageHub.AvailableChargeReceiptData as acrd
         inner join Charges.MarketParticipant as mp
            on acrd.DeprecatedActorId = mp.Id
    
    update acld
    set acld.ActorId = mp.ActorId
    from MessageHub.AvailableChargeLinksData as acld
         inner join Charges.MarketParticipant as mp
            on acld.DeprecatedActorId = mp.Id
    
    update aclrd
    set aclrd.ActorId = mp.ActorId
    from MessageHub.AvailableChargeLinksReceiptData as aclrd
         inner join Charges.MarketParticipant as mp
            on aclrd.DeprecatedActorId = mp.Id
    
    ------------------------------------------------------------------------------------------------------------------------
    -- ActorId not null
    ------------------------------------------------------------------------------------------------------------------------
    
    alter table MessageHub.AvailableChargeData
        alter column ActorId uniqueidentifier not null
    go
    
    alter table MessageHub.AvailableChargeReceiptData
        alter column ActorId uniqueidentifier not null
    go
    
    alter table MessageHub.AvailableChargeLinksData
        alter column ActorId uniqueidentifier not null
    go
    
    alter table MessageHub.AvailableChargeLinksReceiptData
        alter column ActorId uniqueidentifier not null
    go
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Remove deprecated foreign key
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
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Remove DeprecatedActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    alter table MessageHub.AvailableChargeData
        drop column DeprecatedActorId;
    
    alter table MessageHub.AvailableChargeReceiptData
        drop column DeprecatedActorId;
    
    alter table MessageHub.AvailableChargeLinksData
        drop column DeprecatedActorId;
    
    alter table MessageHub.AvailableChargeLinksReceiptData
        drop column DeprecatedActorId;
    
    ------------------------------------------------------------------------------------------------------------------------
    -- New foreign key
    ------------------------------------------------------------------------------------------------------------------------
    
    alter table MessageHub.AvailableChargeData
        with check add constraint FK_AvailableChargeData_MarketParticipantActorId foreign key([ActorId])
            references Charges.MarketParticipant ([ActorId])
    go
    
    alter table MessageHub.AvailableChargeReceiptData
        with check add constraint FK_AvailableChargeReceiptData_MarketParticipantActorId foreign key([ActorId])
            references Charges.MarketParticipant ([ActorId])
    go
    
    alter table MessageHub.AvailableChargeLinksData
        with check add constraint FK_AvailableChargeLinksData_MarketParticipantActorId foreign key([ActorId])
            references Charges.MarketParticipant ([ActorId])
    go
    
    alter table MessageHub.AvailableChargeLinksReceiptData
        with check add constraint FK_AvailableChargeLinksReceiptData_MarketParticipantActorId foreign key([ActorId])
            references Charges.MarketParticipant ([ActorId])
    go

commit