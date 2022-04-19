------------------------------------------------------------------------------------------------------------------------
-- Drop dbo.Duplicates table that was created in a previous script 
-- '202202151610 Move charge periods to ChargePeriod table'
------------------------------------------------------------------------------------------------------------------------
DROP TABLE IF EXISTS dbo.Duplicates
    GO

------------------------------------------------------------------------------------------------------------------------
-- Ensure charge data consistency by removing charge duplicates (and any charge links to them)
------------------------------------------------------------------------------------------------------------------------
begin tran

select
    Id,
    row_number() over(partition by SenderProvidedChargeId, [Type], OwnerId order by SenderProvidedChargeId, [Type], OwnerId) as rownumber
into #Duplicates
from Charges.Charge

    delete cl
    from Charges.ChargeLink as cl
        inner join #Duplicates as dpl
        on dpl.Id = cl.ChargeId
    where dpl.rownumber > 1

    delete cp
    from Charges.ChargePoint as cp
        inner join #Duplicates as dpl
        on dpl.Id = cp.ChargeId
    where dpl.rownumber > 1

	delete cpd
    from Charges.ChargePeriod as cpd
        inner join #Duplicates as dpl
        on dpl.Id = cpd.ChargeId
    where dpl.rownumber > 1

    delete c
    from Charges.Charge as c
        inner join #Duplicates as dpl
        on dpl.Id = c.Id
    where dpl.rownumber > 1

commit

------------------------------------------------------------------------------------------------------------------------
-- Add unique constraint on Charges.Charge to reject inserts for the same unique charge,
-- identified by SenderProvidedChargeId, Type and OwnerId
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[Charge]
    ADD CONSTRAINT [UC_SenderProvidedChargeId_Type_OwnerId] UNIQUE (SenderProvidedChargeId, Type, OwnerId);
GO