IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'UQ_DefaultOverlap_StartDateTime' AND TYPE='UQ')
BEGIN
    ALTER TABLE [Charges].[ChargeLink] DROP CONSTRAINT UQ_DefaultOverlap_StartDateTime
END
GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_StartDateTime UNIQUE (ChargeInformationId, MeteringPointId, StartDateTime);
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'UQ_DefaultOverlap_EndDateTime' AND TYPE='UQ')
BEGIN
    ALTER TABLE [Charges].[ChargeLink] DROP CONSTRAINT UQ_DefaultOverlap_EndDateTime
END
GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_EndDateTime UNIQUE (ChargeInformationId, MeteringPointId, EndDateTime);
GO
