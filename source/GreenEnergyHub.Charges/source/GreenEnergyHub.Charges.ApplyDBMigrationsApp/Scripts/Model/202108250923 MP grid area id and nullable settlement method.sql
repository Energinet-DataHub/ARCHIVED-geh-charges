EXEC sp_rename 'Charges.MeteringPoint.MeteringGridArea', 'MeteringGridAreaId', 'COLUMN';

GO

ALTER TABLE [Charges].[MeteringPoint] ALTER COLUMN SettlementMethod INT NULL