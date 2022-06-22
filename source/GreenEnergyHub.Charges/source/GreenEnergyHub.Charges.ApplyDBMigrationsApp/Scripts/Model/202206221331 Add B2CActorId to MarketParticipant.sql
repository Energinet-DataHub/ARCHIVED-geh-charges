﻿begin tran

    ------------------------------------------------------------------------------------------------------------------------
    -- Add B2CActorId
    ------------------------------------------------------------------------------------------------------------------------

    ALTER TABLE [Charges].[MarketParticipant]
        ADD B2CActorId [uniqueidentifier]
        GO
    
    ------------------------------------------------------------------------------------------------------------------------
    -- Migrate ActorId to B2CActorId
    ------------------------------------------------------------------------------------------------------------------------
    
    UPDATE [Charges].[MarketParticipant]
    SET B2CActorId = Id
    
    select * from [Charges].[MarketParticipant]

commit