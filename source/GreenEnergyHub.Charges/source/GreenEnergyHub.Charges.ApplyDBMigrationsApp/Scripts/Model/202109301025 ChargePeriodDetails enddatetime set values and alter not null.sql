UPDATE [Charges].[ChargePeriodDetails]
SET EndDateTime = '9999-12-31 23:59:59' -- Equivalent to InstantExtensions.TimeOrEndDefault()
WHERE EndDateTime is null

DROP INDEX [i1] ON [Charges].[ChargePeriodDetails]
GO

ALTER TABLE [Charges].[ChargePeriodDetails] ALTER COLUMN EndDateTime datetime2 NOT NULL
GO

CREATE INDEX [IX_ChargeRowId_StartDateTime_EndDateTime_Retired] ON [Charges].[ChargePeriodDetails]
    (ChargeRowId DESC, StartDateTime DESC, EndDateTime DESC, [Retired] DESC);
GO
