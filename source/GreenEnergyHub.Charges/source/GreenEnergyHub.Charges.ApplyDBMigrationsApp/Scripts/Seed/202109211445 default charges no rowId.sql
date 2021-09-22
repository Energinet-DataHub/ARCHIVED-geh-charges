/* We need to be careful with seed data, as the data might have already added by manual or automatic tests or by migration earlier. So we need to create only what is needed */

/* Charge 40000 */
BEGIN
  IF NOT EXISTS (
    SELECT SenderProvidedChargeId FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = '40000'
        AND MarketParticipantId IN (SELECT MarketParticipantId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(), /* Id */
      '40000', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '40000'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '40000'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Transmissions nettarif', /* Name */
      'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = '40000'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 40010 */
BEGIN
  IF NOT EXISTS (
    SELECT SenderProvidedChargeId FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = '40010'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(), /* Id */
      '40010', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '40010'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '40010'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Indfødningstarif produktion', /* Name */
      'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = '40010'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 41000 */
BEGIN
  IF NOT EXISTS (
    SELECT Id FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = '41000'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(), /* Id */
      '41000', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '41000'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '41000'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Systemtarif', /* Name */
      'Systemafgiften dækker omkostninger til forsyningssikkerhed og elforsyningens kvalitet.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = '41000'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 45012 */
BEGIN
  IF NOT EXISTS (
    SELECT Id FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = '45012'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(),
      '45012', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '45012'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '45012'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Balancetarif for produktion', /* Name */
      'Balancetarif for produktion', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = '45012'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 45013 */
BEGIN
  IF NOT EXISTS (
    SELECT Id FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = '45013'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(),
      '45013', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '45013'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = '45013'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Balancetarif for forbrug', /* Name */
      'Balancetarif for forbrug', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = '45013'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge EA-001 */
BEGIN
  IF NOT EXISTS (
    SELECT Id FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = 'EA-001'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	  NEWID(),
      'EA-001', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      1, /* TaxIndicator, yes */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      1 /* TransparentInvoicing, yes */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = 'EA-001'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = 'EA-001'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Elafgift', /* Name */
      'Elafgiften', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = 'EA-001'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge EA-003 */
BEGIN
  IF NOT EXISTS (
    SELECT Id FROM [Charges].[Charge]
      WHERE
        SenderProvidedChargeId = 'EA-003'
        AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, ChargeType, MarketParticipantId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
	 NEWID(),
      'EA-003', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT Id FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantId */
      1, /* TaxIndicator, yes */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (Id, ChargeId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = 'EA-003'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (Id, ChargeId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationId) VALUES (
	  NEWID(),
      (SELECT Id FROM [Charges].[Charge]
        WHERE
          SenderProvidedChargeId = 'EA-003'
          AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Negativt reduceret elafgift', /* Name */
      'Negativ reduceret elafgift for elvarmekunder til brug for modregning af den almindelige reducerede elafgift for elvarmekunder. Bruges kun sammen med D14 målepunkter.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT Id FROM [Charges].[ChargeOperation]
        WHERE ChargeId IN (
          SELECT Id FROM [Charges].[Charge]
            WHERE
              SenderProvidedChargeId = 'EA-003'
              AND MarketParticipantId IN (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END
GO