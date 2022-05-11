------------------------------------------------------------------------------------------------------------------------
-- Rename Charge table to ChargeInformation
------------------------------------------------------------------------------------------------------------------------
if EXISTS(SELECT 1 FROM sys.tables WHERE NAME = 'Charge' AND Type='U')
BEGIN
  EXEC sp_rename 'Charges.Charge', 'ChargeInformation';
END
GO


-- ChargeLink
IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_ChargeLink_Charge' AND TYPE='F')
BEGIN
    EXEC sp_rename 'Charges.FK_ChargeLink_Charge', 'FK_ChargeLink_ChargeInformation';
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ChargeId' AND Object_ID = Object_ID(N'Charges.ChargeLink'))
BEGIN
    EXEC sp_rename 'Charges.ChargeLink.ChargeId', 'ChargeInformationId', 'COLUMN';
END
GO

-- ChargePeriod
IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_ChargePeriod_Charge' AND TYPE='F')
BEGIN
    EXEC sp_rename 'Charges.FK_ChargePeriod_Charge', 'FK_ChargePeriod_ChargeInformation';
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ChargeId' AND Object_ID = Object_ID(N'Charges.ChargePeriod'))
BEGIN
    EXEC sp_rename 'Charges.ChargePeriod.ChargeId', 'ChargeInformationId', 'COLUMN';
END
GO

-- ChargePoint
IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_ChargePoint_Charge' AND TYPE='F')
BEGIN
    EXEC sp_rename 'Charges.FK_ChargePoint_Charge', 'FK_ChargePoint_ChargeInformation';
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ChargeId' AND Object_ID = Object_ID(N'Charges.ChargePoint'))
BEGIN
    EXEC sp_rename 'Charges.ChargePoint.ChargeId', 'ChargeInformationId', 'COLUMN';
END
GO

-- DefaultChargeLink
IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_DefaultChargeLink_Charge' AND TYPE='F')
BEGIN
    EXEC sp_rename 'Charges.FK_DefaultChargeLink_Charge', 'FK_DefaultChargeLink_ChargeInformation';
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ChargeId' AND Object_ID = Object_ID(N'Charges.DefaultChargeLink'))
BEGIN
    EXEC sp_rename 'Charges.DefaultChargeLink.ChargeId', 'ChargeInformationId', 'COLUMN';
END
GO
