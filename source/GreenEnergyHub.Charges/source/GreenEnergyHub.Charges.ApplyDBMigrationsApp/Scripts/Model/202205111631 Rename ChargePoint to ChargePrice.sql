------------------------------------------------------------------------------------------------------------------------
-- Rename ChargePoint table to ChargePrice
------------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'Charges.ChargePoint', 'ChargePrice';
GO

-- ChargePrice
IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_ChargePoint_ChargeInformation' AND TYPE='PK')
BEGIN
    EXEC sp_rename 'Charges.PK_ChargePoint', 'PK_ChargePrice';
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE NAME = 'FK_ChargePoint_ChargeInformation' AND TYPE='F')
BEGIN
    EXEC sp_rename 'Charges.FK_ChargePoint_ChargeInformation', 'FK_ChargePrice_ChargeInformation';
END
GO


