begin tran

    ------------------------------------------------------------------------------------------------------------------------
    -- Add ActorId and B2CActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    ALTER TABLE [Charges].[MarketParticipant]
    ADD ActorId [uniqueidentifier] not null default(newid()),
        B2CActorId [uniqueidentifier]
    GO
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Migrate Id to ActorId and B2CActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    UPDATE [Charges].[MarketParticipant]
    SET B2CActorId = Id,
        ActorId = Id

commit