/* This seed is added in the same PR as the as table with settings and is therefore not dangerous in regards to existing data or migration */

/* Charge 40000, consumption */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  1, /* MeteringPointType, Consumption */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = '40000'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 40010, production */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  2, /* MeteringPointType, Production */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = '40010'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */

/* Charge 41000, consumption */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  1, /* MeteringPointType, Consumption */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = '41000'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 45012, production */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  2, /* MeteringPointType, Production */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = '45012'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge 45013, consumption */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  1, /* MeteringPointType, Consumption */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = '45013'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-001, consumption */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  1, /* MeteringPointType, Consumption */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = 'EA-001'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-001, electrical heating */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = 'EA-001'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
  
/* Charge EA-003, electrical heating */
INSERT INTO [Charges].[DefaultChargeLinkSetting] (MeteringPointType, ChargeRowId, StartDateTime, EndDateTime) VALUES (
  16, /* MeteringPointType, ElectricalHeating */
  (SELECT RowId FROM [Charges].[Charge]
    WHERE
      ChargeId = 'EA-003'
      AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
      AND ChargeType = 3), /* ChargeRowId */
  '2014-12-31 23:00', /* StartDateTime */
  NULL) /* EndDateTime */
GO