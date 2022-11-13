------------------------------------------------------------------------------------------------------------------------
-- Add System versioning to Charge table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[Charge] ADD [ValidFrom] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
ALTER TABLE [Charges].[Charge] ADD [ValidTo] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
GO

ALTER TABLE [Charges].[Charge] ADD PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
ALTER TABLE [Charges].[Charge] ALTER COLUMN [ValidFrom] ADD HIDDEN
ALTER TABLE [Charges].[Charge] ALTER COLUMN [ValidTo] ADD HIDDEN
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Charges].[Charge] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[ChargeHistory]))')

GO

------------------------------------------------------------------------------------------------------------------------
-- Add System versioning to ChargePeriod table
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[ChargePeriod] ADD [ValidFrom] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
ALTER TABLE [Charges].[ChargePeriod] ADD [ValidTo] datetime2 NOT NULL DEFAULT '9999-12-31T23:59:59.9999999';
GO

ALTER TABLE [Charges].[ChargePeriod] ADD PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
ALTER TABLE [Charges].[ChargePeriod] ALTER COLUMN [ValidFrom] ADD HIDDEN
ALTER TABLE [Charges].[ChargePeriod] ALTER COLUMN [ValidTo] ADD HIDDEN
DECLARE @historyTableSchema sysname = SCHEMA_NAME()
EXEC(N'ALTER TABLE [Charges].[ChargePeriod] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[ChargePeriodHistory]))')
