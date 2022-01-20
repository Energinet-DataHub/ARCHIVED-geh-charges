-- Update to current model restrictions
alter table Charges.Charge
alter column EndDateTime DateTime2 not null