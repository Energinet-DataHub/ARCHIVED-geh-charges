/* Charge 40000, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '40000'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge 40010, production */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  2, /* MeteringPointType, Production */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '40010'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */

/* Charge 41000, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '41000'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge 45012, production */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  2, /* MeteringPointType, Production */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '45012'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge 45013, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '45013'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge EA-001, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-001'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge EA-001, electrical heating */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-001'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
  
/* Charge EA-003, electrical heating */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-003'
      AND OwnerId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND Type = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59') /* EndDateTime */
