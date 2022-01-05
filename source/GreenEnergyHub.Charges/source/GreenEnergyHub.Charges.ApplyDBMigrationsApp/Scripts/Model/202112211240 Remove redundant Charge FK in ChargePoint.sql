-- At the moment there are two foreign keys in the ChargePoint table; 'FK_Charge' and one that has an auto-generated name 'FK__ChargePri__Charg__' with a random ID at the end.
-- This script will remove the FK with the auto-generated name.

-- Get the complete auto-generated FK name
DECLARE @dropForeignKeyCommand nvarchar(100)
SELECT @dropForeignKeyCommand = [name] FROM sys.foreign_keys where name like 'FK__ChargePri__Charg__%'

-- Removes the auto-generated FK
DECLARE @dropCommand nvarchar(200)
SELECT @dropCommand = 'ALTER TABLE [Charges].[ChargePoint] DROP CONSTRAINT ' + @dropForeignKeyCommand
EXEC sp_executesql @dropCommand
