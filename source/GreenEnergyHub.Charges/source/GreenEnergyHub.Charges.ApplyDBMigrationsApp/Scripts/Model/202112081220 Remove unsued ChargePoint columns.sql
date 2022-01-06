ALTER TABLE [Charges].[ChargePoint]
DROP COLUMN Retired
GO

ALTER TABLE [Charges].[ChargePoint]
DROP COLUMN RetiredDateTime
GO

DROP INDEX IX_ChargeId_Time_ChargeOperationId ON Charges.ChargePoint;

-- Need to drop FK with dynamic name on column ChargeOperationRowId before deleting the column.
-- No easy way, so remove both constraints and then recreate the one that should stay (but this time with a name!)
DECLARE @dropCommand nvarchar(1000)
SELECT @dropCommand = 'alter table ' + schema_name(Schema_id)+'.'+ object_name(parent_object_id) + '  DROP CONSTRAINT  ' +  name
from sys.foreign_keys
where schema_name(Schema_id)+'.'+ object_name(parent_object_id) = 'Charges.ChargePoint'

EXEC sp_executesql @dropCommand

ALTER TABLE Charges.ChargePoint
ADD CONSTRAINT FK_Charge
FOREIGN KEY (ChargeId) REFERENCES Charges.Charge(Id);

ALTER TABLE [Charges].[ChargePoint]
DROP COLUMN ChargeOperationId
GO
