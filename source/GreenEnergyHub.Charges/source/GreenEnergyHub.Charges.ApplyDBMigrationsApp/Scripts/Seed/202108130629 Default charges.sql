/* We need to be careful with seed data, as the data might have already added by manual or automatic tests or by migration earlier. So we need to create only what is needed */

/* Charge 40000 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = '40000'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      '40000', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '40000'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '40000'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Transmissions nettarif', /* Name */
      'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = '40000'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 40010 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = '40010'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      '40010', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '40010'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '40010'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Indfødningstarif produktion', /* Name */
      'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = '40010'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 41000 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = '41000'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      '41000', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '41000'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '41000'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Systemtarif', /* Name */
      'Systemafgiften dækker omkostninger til forsyningssikkerhed og elforsyningens kvalitet.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = '41000'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 45012 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = '45012'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      '45012', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '45012'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '45012'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Balancetarif for produktion', /* Name */
      'Balancetarif for produktion', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = '45012'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge 45013 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = '45013'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      '45013', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      0, /* TaxIndicator, no */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '45013'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = '45013'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Balancetarif for forbrug', /* Name */
      'Balancetarif for forbrug', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = '45013'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge EA-001 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = 'EA-001'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      'EA-001', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      1, /* TaxIndicator, yes */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      1 /* TransparentInvoicing, yes */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = 'EA-001'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = 'EA-001'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Elafgift', /* Name */
      'Elafgiften', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = 'EA-001'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END

/* Charge EA-003 */
BEGIN
  IF NOT EXISTS (
    SELECT ChargeId FROM [Charges].[Charge]
      WHERE
        ChargeId = 'EA-003'
        AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
        AND ChargeType = 3 /* Tariff */
  )
  BEGIN
    INSERT INTO [Charges].[Charge] (ChargeId, ChargeType, MarketParticipantRowId, TaxIndicator, Resolution, Currency, TransparentInvoicing) VALUES (
      'EA-003', /* ChargeId */ 
      3, /* ChargeType, Tariff */
      (SELECT RowId FROM [Charges].[MarketParticipant]
        WHERE
          MarketParticipantId = '5790000432752'), /* MarketParticipantRowId */
      1, /* TaxIndicator, yes */
      2, /* Resolution, P1D */
      'DKK', /* Currency */
      0 /* TransparentInvoicing, no */
    )

    INSERT INTO [Charges].[ChargeOperation] (ChargeRowId, CorrelationId, ChargeOperationId, WriteDateTime) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = 'EA-003'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      NEWID(), /* CorrelationId */
      CONCAT('Seed ', GETUTCDATE()), /* ChargeOperationId */
      CONVERT(datetime, GETUTCDATE())) /* WriteDateTime */

    INSERT INTO [Charges].[ChargePeriodDetails] (ChargeRowId, StartDateTime, EndDateTime, Name, Description, VatClassification, Retired, RetiredDateTime, ChargeOperationRowId) VALUES (
      (SELECT RowId FROM [Charges].[Charge]
        WHERE
          ChargeId = 'EA-003'
          AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
          AND ChargeType = 3), /* ChargeRowId */
      '2014-12-31 23:00', /* StartDateTime */
      NULL, /* EndDateTime */
      'Negativt reduceret elafgift', /* Name */
      'Negativ reduceret elafgift for elvarmekunder til brug for modregning af den almindelige reducerede elafgift for elvarmekunder. Bruges kun sammen med D14 målepunkter.', /* Description */
      2, /* VatClassification, 25% */
      0, /* Retired, no */
      NULL, /* RetiredDateTime */
      (SELECT RowId FROM [Charges].[ChargeOperation]
        WHERE ChargeRowId IN (
          SELECT RowId FROM [Charges].[Charge]
            WHERE
              ChargeId = 'EA-003'
              AND MarketParticipantRowId IN (SELECT RowId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
              AND ChargeType = 3)))
  END
END
GO