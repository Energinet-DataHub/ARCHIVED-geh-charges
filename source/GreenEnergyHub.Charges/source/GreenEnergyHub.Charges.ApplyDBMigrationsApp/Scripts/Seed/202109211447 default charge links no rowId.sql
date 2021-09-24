/* This seed is added in the same PR as the as table with settings and is therefore not dangerous in regards to existing data or migration */

/* Charge 40000, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '40000'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 40010, production */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  2, /* MeteringPointType, Production */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '40010'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */

/* Charge 41000, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '41000'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 45012, production */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  2, /* MeteringPointType, Production */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '45012'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 45013, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = '45013'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-001, consumption */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  1, /* MeteringPointType, Consumption */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-001'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-001, electrical heating */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-001'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-003, electrical heating */
INSERT INTO [Charges].[DefaultChargeLink] (Id, MeteringPointType, ChargeId, StartDateTime, EndDateTime) VALUES (
  NEWID(),
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT Id FROM [Charges].[Charge]
    WHERE
      SenderProvidedChargeId = 'EA-003'
      AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
GO