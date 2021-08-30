-- ### ChargeOperation ###

DROP INDEX i1 on Charges.ChargeOperation 
GO

ALTER TABLE Charges.ChargeOperation 
ALTER COLUMN WriteDateTime DateTime2 NOT NULL

CREATE INDEX i1 ON [Charges].[ChargeOperation] (ChargeRowId DESC, WriteDateTime DESC);

-- ### ChargePeriodDetails ###

DROP INDEX i1 on Charges.ChargePeriodDetails 
GO

ALTER TABLE Charges.ChargePeriodDetails 
ALTER COLUMN StartDateTime DateTime2 NOT NULL

ALTER TABLE Charges.ChargePeriodDetails 
ALTER COLUMN EndDateTime DateTime2 NULL

ALTER TABLE Charges.ChargePeriodDetails 
ALTER COLUMN RetiredDateTime DateTime2 NULL

CREATE INDEX i1 ON [Charges].[ChargePeriodDetails] (ChargeRowId DESC, StartDateTime DESC, EndDateTime DESC, Retired DESC);

-- ### ChargePrice ###

DROP INDEX i1 on Charges.ChargePrice 
GO

ALTER TABLE Charges.ChargePrice 
ALTER COLUMN Time DateTime2 NOT NULL

ALTER TABLE Charges.ChargePrice
ALTER COLUMN RetiredDateTime DateTime2 NULL

CREATE INDEX i1 ON [Charges].[ChargePrice] (ChargeRowId DESC, Time DESC);

-- ### DefaultChargeLinkSetting ###

DROP INDEX i1 on Charges.DefaultChargeLinkSetting 
GO

ALTER TABLE Charges.DefaultChargeLinkSetting 
ALTER COLUMN StartDateTime DateTime2 NOT NULL

ALTER TABLE Charges.DefaultChargeLinkSetting 
ALTER COLUMN EndDateTime DateTime2 NULL

CREATE INDEX i1 ON [Charges].[DefaultChargeLinkSetting] (MeteringPointType ASC, StartDateTime DESC, EndDateTime DESC);

-- ### MeteringPoint ### 

ALTER TABLE Charges.MeteringPoint 
ALTER COLUMN EffectiveDate DateTime2 NOT NULL
