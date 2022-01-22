/* Charge 40000 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(), /* Id */
  '40000', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  0, /* TaxIndicator, no */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Transmissions nettarif', /* Name */
  'Netafgiften, for b�de forbrugere og producenter, d�kker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge 40010 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(), /* Id */
  '40010', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  0, /* TaxIndicator, no */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Indf�dningstarif produktion', /* Name */
  'Netafgiften, for b�de forbrugere og producenter, d�kker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne.', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge 41000 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(), /* Id */
  '41000', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  0, /* TaxIndicator, no */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Systemtarif', /* Name */
  'Systemafgiften d�kker omkostninger til forsyningssikkerhed og elforsyningens kvalitet.', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge 45012 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(),
  '45012', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  0, /* TaxIndicator, no */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Balancetarif for produktion', /* Name */
  'Balancetarif for produktion', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge 45013 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(),
  '45013', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  0, /* TaxIndicator, no */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Balancetarif for forbrug', /* Name */
  'Balancetarif for forbrug', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge EA-001 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
  NEWID(),
  'EA-001', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  1, /* TaxIndicator, yes */
  2, /* Resolution, P1D */
  1, /* TransparentInvoicing, yes */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Elafgift', /* Name */
  'Elafgiften', /* Description */
  2 /* VatClassification, 25% */
)

/* Charge EA-003 */
INSERT INTO [Charges].[Charge] (Id, SenderProvidedChargeId, [Type], OwnerId, TaxIndicator, Resolution, TransparentInvoicing, StartDateTime, EndDateTime, Name, Description, VatClassification) VALUES (
 NEWID(),
  'EA-003', /* ChargeId */ 
  3, /* [Type], Tariff */
  (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752'), /* MarketParticipantId */
  1, /* TaxIndicator, yes */
  2, /* Resolution, P1D */
  0, /* TransparentInvoicing, no */
  '2014-12-31 23:00', /* StartDateTime */
  '9999-12-31 23:59:59', -- Equivalent to InstantExtensions.TimeOrEndDefault()
  'Negativt reduceret elafgift', /* Name */
  'Negativ reduceret elafgift for elvarmekunder til brug for modregning af den almindelige reducerede elafgift for elvarmekunder. Bruges kun sammen med D14 m�lepunkter.', /* Description */
  2 /* VatClassification, 25% */
)