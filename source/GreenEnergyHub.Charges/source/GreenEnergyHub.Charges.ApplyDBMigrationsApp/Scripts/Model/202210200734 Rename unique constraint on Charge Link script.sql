------------------------------------------------------------------------------------------------------------------------
-- Rename delete old unique constraint on ChargeLink table script
------------------------------------------------------------------------------------------------------------------------

delete sv
from [dbo].[SchemaVersions] sv
where sv.ScriptName = 'GreenEnergyHub.Charges.ApplyDBMigrationsApp.Scripts.Model.280320221347 Add unique constraint on Charge Link.sql'