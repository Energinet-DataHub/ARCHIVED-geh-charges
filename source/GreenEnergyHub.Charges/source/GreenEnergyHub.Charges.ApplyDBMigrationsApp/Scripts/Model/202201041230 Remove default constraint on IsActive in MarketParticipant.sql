-- At the moment there is a default constaint in the MarketParticipant table with an auto-generated name 'DF__MarketPar__Activ__' with a random ID at the end.
-- This script will remove the default constraint with the auto-generated name.

-- Get the complete auto-generated default constraint
DECLARE @dropDefaultConstraint nvarchar(100)
SELECT @dropDefaultConstraint = [name] FROM sys.default_constraints where name like 'DF__MarketPar__Activ__%'

-- Removes the auto-generated default constraint
DECLARE @dropCommand nvarchar(200)
SELECT @dropCommand = 'ALTER TABLE [Charges].[MarketParticipant] DROP CONSTRAINT ' + @dropDefaultConstraint
EXEC sp_executesql @dropCommand
