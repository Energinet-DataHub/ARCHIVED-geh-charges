-- Drop FK with dynamic name on column ChargeOperationRowId
-- No easy way, so remove both constraints and then recreate the one that should stay (but this time with a name!)
DECLARE @dropCommand nvarchar(1000)
SELECT @dropCommand = 'alter table ' + schema_name(Schema_id)+'.'+ object_name(parent_object_id) + '  DROP CONSTRAINT  ' +  name
from sys.foreign_keys
where schema_name(Schema_id)+'.'+ object_name(parent_object_id) = 'Charges.ChargePoint'

EXEC sp_executesql @dropCommand
