
UPDATE [Charges].[DefaultChargeLink]
SET EndDateTime = '9999-12-31 23:59:59' -- Equivalent to InstantExtensions.TimeOrEndDefault()
WHERE EndDateTime is null

DROP INDEX [i1] ON [Charges].[DefaultChargeLink]
GO

ALTER TABLE [Charges].[DefaultChargeLink] ALTER COLUMN EndDateTime datetime2 NOT NULL
GO

CREATE INDEX [IX_MeteringPointType_StartDateTime_EndDateTime] ON [Charges].[DefaultChargeLink]
    (MeteringPointType ASC, StartDateTime DESC, EndDateTime DESC);
GO
