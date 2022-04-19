------------------------------------------------------------------------------------------------------------------------
-- Ensure charge data consistency, before adding a unique constraint to the Charge table
------------------------------------------------------------------------------------------------------------------------

begin tran

select
    Id,
    row_number() over(partition by SenderProvidedChargeId, [Type], OwnerId order by SenderProvidedChargeId, [Type], OwnerId) as rownumber
into Duplicates1
from Charges.Charge

delete cl
    from Charges.ChargeLink as cl
        inner join Duplicates1 as dpl
        on dpl.Id = cl.ChargeId
    where dpl.rownumber > 1

    delete cp
    from Charges.ChargePoint as cp
        inner join Duplicates1 as dpl
        on dpl.Id = cp.ChargeId
    where dpl.rownumber > 1

	delete cpd
    from Charges.ChargePeriod as cpd
        inner join Duplicates1 as dpl
        on dpl.Id = cpd.ChargeId
    where dpl.rownumber > 1

    delete c
    from Charges.Charge as c
        inner join Duplicates1 as dpl
        on dpl.Id = c.Id
    where dpl.rownumber > 1

select
    Id,
    row_number() over(partition by SenderProvidedChargeId, [Type], OwnerId order by SenderProvidedChargeId, [Type], OwnerId) as rownumber
from Charges.Charge
order by rownumber desc

commit

------------------------------------------------------------------------------------------------------------------------
-- Add unique constraint on Charge table entries; Charge must be unique on SenderProvidedChargeId, Type and OwnerId
------------------------------------------------------------------------------------------------------------------------